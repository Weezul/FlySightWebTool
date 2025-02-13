using System;

namespace FlySightWebTool.Data;

public class TrackLog
{
    //Logged data
    public DateTime Time { get; set; } //Time in ISO8601 format
    public double Latitude { get; set; } //Latitude (degrees)
    public double Longitude { get; set; } //Longitude (degrees)
    public double Altitude { get; set; } //Altitude AMLS (m)
    public double VelocityNorth { get; set; } //Velocity North (m/s)
    public double VelocityEast { get; set; } //Velocity East (m/s)
    public double VelocityDown { get; set; } //Velocity Down (m/s)
    public double AccuracyHorizontal { get; set; } //Horizontal accuracy (m)
    public double AccuracyVertical { get; set; } //Vertical accuracy (m)
    public double AccuracySpeed { get; set; } //Speed accuracy (m/s)
    public int NumberOfSatellites { get; set; } //Number of satellites used in fix

    //Computed data
    public double VelocityTotal { get; set; } //Velocity total (m/s)
    public double VelocityGround { get; set; } //Velocity over ground plane (m/s)
    //TODO: Add angle
    public double AccelerationNorth { get; set; } //Acceleration North (m/s/s)
    public double AccelerationEast { get; set; } //Acceleration East (m/s/s)
    public double AccelerationDown { get; set; } //Acceleration Down (m/s/s)
    public double AccelerationTotal { get; set; } //Acceleration Total (m/s/s)
    public double AccelerationGround { get; set; } //Acceleration over ground plane (m/s/s)
    public double Height { get; set; } //Height AGL (m)
    public double FlightTimeStamp { get; set; } //Flight time (s)
    public double GlideRatio { get; set; } //Glide ration
    public double VelocityDownKmh => VelocityDown * 3.6; //Velocity down (km/h)
    public double VelocityGroundKmh => VelocityGround * 3.6; //Velocity ground (km/h)
    public double VelocityTotalKmh => VelocityTotal * 3.6; //Velocity total (km/h)
    public double HorizontalDistance { get; set; } //Horizontal distance (m)

    public static TrackLog FromCsvLine(string csvLine)
    {
        var values = csvLine.Split(',');
        if (values.Length < 12 || !values[0].StartsWith("$GNSS"))
            throw new FormatException("Invalid CSV line format");

        //Recorded
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

        //Computed
        trackLog.VelocityTotal = Math.Sqrt(
            Math.Pow(trackLog.VelocityNorth, 2) +
            Math.Pow(trackLog.VelocityEast, 2) +
            Math.Pow(trackLog.VelocityDown, 2));

        trackLog.VelocityGround = Math.Sqrt(
            Math.Pow(trackLog.VelocityNorth, 2) +
            Math.Pow(trackLog.VelocityEast, 2));

        return trackLog;
    }

    public void computeRelative(TrackLog prevTrackLog, double dzAltitude, DateTime exitDateTime)
    {
        Height = Altitude - dzAltitude;

        double timeDelta = (Time - prevTrackLog.Time).TotalSeconds; // Corrected time delta calculation
        AccelerationDown = (VelocityDown - prevTrackLog.VelocityDown) / timeDelta;
        AccelerationEast = (VelocityEast - prevTrackLog.VelocityEast) / timeDelta;
        AccelerationNorth = (VelocityNorth - prevTrackLog.VelocityNorth) / timeDelta;

        AccelerationTotal = Math.Sqrt(
            AccelerationDown * AccelerationDown + 
            AccelerationEast * AccelerationEast +
            AccelerationNorth * AccelerationNorth);

        AccelerationGround = Math.Sqrt(
            AccelerationEast * AccelerationEast +
            AccelerationNorth * AccelerationNorth);

        FlightTimeStamp = exitDateTime > DateTime.MinValue ? (Time - exitDateTime).TotalSeconds : 0.0;
        HorizontalDistance = calculateHorizontalDistance(prevTrackLog);
        GlideRatio = calculateGlideRatio(prevTrackLog);
    }

    private double calculateHorizontalDistance(TrackLog prevTrackLog)
    {
        return HaversineDistance(prevTrackLog.Latitude, prevTrackLog.Longitude, Latitude, Longitude); // in meters
    }

    private double calculateGlideRatio(TrackLog prevTrackLog)
    {
        double altitudeChange = prevTrackLog.Altitude - Altitude; // Negative if climbing

        if (HorizontalDistance == 0.0)
            HorizontalDistance = calculateHorizontalDistance(prevTrackLog);

        return HorizontalDistance / altitudeChange;
    }

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

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;
}