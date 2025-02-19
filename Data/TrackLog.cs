using System;

namespace FlySightWebTool.Data
{
    public enum FlightPhase
    {
        Boarding,
        Aircraft,
        Freefall,
        Canopy,
        Landed
    }

    public class TrackLog
    {
        // Logged data
        public DateTime Time { get; set; } // Time in ISO8601 format
        public double Latitude { get; set; } // Latitude (degrees)
        public double Longitude { get; set; } // Longitude (degrees)
        public double Altitude { get; set; } // Altitude AMLS (m)
        public double VelocityNorth { get; set; } // Velocity North (m/s)
        public double VelocityEast { get; set; } // Velocity East (m/s)
        public double VelocityDown { get; set; } // Velocity Down (m/s)
        public double AccuracyHorizontal { get; set; } // Horizontal accuracy (m)
        public double AccuracyVertical { get; set; } // Vertical accuracy (m)
        public double AccuracySpeed { get; set; } // Speed accuracy (m/s)
        public int NumberOfSatellites { get; set; } // Number of satellites used in fix

        // Computed data
        public double VelocityTotal { get; set; } // Velocity total (m/s)
        public double VelocityGround { get; set; } // Velocity over ground plane (m/s)        
        public double AccelerationNorth { get; set; } // Acceleration North (m/s/s)
        public double AccelerationEast { get; set; } // Acceleration East (m/s/s)
        public double AccelerationDown { get; set; } // Acceleration Down (m/s/s)
        public double AccelerationTotal { get; set; } // Acceleration Total (m/s/s)
        public double AccelerationGround { get; set; } // Acceleration over ground plane (m/s/s)
        public double Height { get; set; } // Height AGL (m)
        public double FlightTimeStamp { get; set; } // Flight time (s)
        public double GlideRatio { get; set; } // Glide ratio
        public double VelocityDownKmh => VelocityDown * 3.6; // Velocity down (km/h)
        public double VelocityGroundKmh => VelocityGround * 3.6; // Velocity ground (km/h)
        public double VelocityTotalKmh => VelocityTotal * 3.6; // Velocity total (km/h)
        public double HorizontalDistance { get; set; } // Horizontal distance (m)
        public FlightPhase Phase; //Flight phase

        /// <summary>
        /// Creates a TrackLog instance from a CSV line.
        /// </summary>
        /// <param name="csvLine">The CSV line containing track log data.</param>
        /// <returns>A TrackLog instance.</returns>
        /// <exception cref="FormatException">Thrown when the CSV line format is invalid.</exception>
        public static TrackLog FromCsvLine(string csvLine)
        {
            var values = csvLine.Split(',');
            if (values.Length < 12 || !values[0].StartsWith("$GNSS"))
                throw new FormatException("Invalid CSV line format");

            // Recorded
            var trackLog = new TrackLog
            {
                Time = DateTime.Parse(values[1]),
                Latitude = double.Parse(values[2]),
                Longitude = double.Parse(values[3]),
                Altitude = double.Parse(values[4]),
                VelocityNorth = double.Parse(values[5]),
                VelocityEast = double.Parse(values[6]),
                VelocityDown = double.Parse(values[7]),
                AccuracyHorizontal = double.Parse(values[8]),
                AccuracyVertical = double.Parse(values[9]),
                AccuracySpeed = double.Parse(values[10]),
                NumberOfSatellites = int.Parse(values[11])
            };

            // Computed
            trackLog.VelocityTotal = Math.Sqrt(
                Math.Pow(trackLog.VelocityNorth, 2) +
                Math.Pow(trackLog.VelocityEast, 2) +
                Math.Pow(trackLog.VelocityDown, 2));

            trackLog.VelocityGround = Math.Sqrt(
                Math.Pow(trackLog.VelocityNorth, 2) +
                Math.Pow(trackLog.VelocityEast, 2));

            return trackLog;
        }

        /// <summary>
        /// Computes relative values based on the previous track log, drop zone altitude, and exit date/time.
        /// </summary>
        /// <param name="prevTrackLog">The previous track log.</param>
        /// <param name="dzAltitude">The drop zone altitude.</param>
        /// <param name="exitDateTime">The exit date/time.</param>
        public void ComputeRelative(TrackLog prevTrackLog, double dzAltitude, DateTime exitDateTime)
        {
            Height = Altitude - dzAltitude;

            double timeDelta = (Time - prevTrackLog.Time).TotalSeconds;
            AccelerationDown = (VelocityDown - prevTrackLog.VelocityDown) / timeDelta;
            AccelerationEast = (VelocityEast - prevTrackLog.VelocityEast) / timeDelta;
            AccelerationNorth = (VelocityNorth - prevTrackLog.VelocityNorth) / timeDelta;

            AccelerationTotal = GetSignedAcceleration(VelocityNorth, VelocityEast, VelocityDown, AccelerationNorth, AccelerationEast, AccelerationDown);
            AccelerationGround = GetSignedAcceleration(VelocityNorth, VelocityEast, 0, AccelerationNorth, AccelerationEast, 0);

            FlightTimeStamp = exitDateTime > DateTime.MinValue ? (Time - exitDateTime).TotalSeconds : 0.0;
            HorizontalDistance = CalculateHorizontalDistance(prevTrackLog);
            GlideRatio = CalculateGlideRatio(prevTrackLog);
        }

        /// <summary>
        /// Computes the signed acceleration.
        /// </summary>
        /// <param name="vN">Velocity North.</param>
        /// <param name="vE">Velocity East.</param>
        /// <param name="vD">Velocity Down.</param>
        /// <param name="aN">Acceleration North.</param>
        /// <param name="aE">Acceleration East.</param>
        /// <param name="aD">Acceleration Down.</param>
        /// <returns>The signed acceleration.</returns>
        private static double GetSignedAcceleration(double vN, double vE, double vD, double aN, double aE, double aD)
        {
            // Compute acceleration magnitude
            double totalAcceleration = Math.Sqrt(aN * aN + aE * aE + aD * aD);

            // Compute dot product
            double dotProduct = vN * aN + vE * aE + vD * aD;

            // Assign sign based on dot product
            return dotProduct < 0 ? -totalAcceleration : totalAcceleration;
        }

        /// <summary>
        /// Calculates the horizontal distance from the previous track log.
        /// </summary>
        /// <param name="prevTrackLog">The previous track log.</param>
        /// <returns>The horizontal distance in meters.</returns>
        private double CalculateHorizontalDistance(TrackLog prevTrackLog)
        {
            return HaversineDistance(prevTrackLog.Latitude, prevTrackLog.Longitude, Latitude, Longitude); // in meters
        }

        /// <summary>
        /// Calculates the glide ratio from the previous track log.
        /// </summary>
        /// <param name="prevTrackLog">The previous track log.</param>
        /// <returns>The glide ratio.</returns>
        private double CalculateGlideRatio(TrackLog prevTrackLog)
        {
            double altitudeChange = prevTrackLog.Altitude - Altitude; // Negative if climbing

            if (HorizontalDistance == 0.0)
                HorizontalDistance = CalculateHorizontalDistance(prevTrackLog);

            return HorizontalDistance / altitudeChange;
        }

        /// <summary>
        /// Calculates the Haversine distance between two points.
        /// </summary>
        /// <param name="lat1">Latitude of the first point.</param>
        /// <param name="lon1">Longitude of the first point.</param>
        /// <param name="lat2">Latitude of the second point.</param>
        /// <param name="lon2">Longitude of the second point.</param>
        /// <returns>The distance in meters.</returns>
        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // Earth radius in meters
            double dLat = DegreesToRadians(lat2 - lat1);
            double dLon = DegreesToRadians(lon2 - lon1);
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                       Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                       Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c; // Distance in meters
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        /// <param name="degrees">The degrees to convert.</param>
        /// <returns>The radians.</returns>
        private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
    }
}