using System;
using System.Linq;
using System.Collections.Generic;

namespace FlySightWebTool.Data
{
    public class Track
    {
        // Properties
        public DateTime TakeOffDateTime { get; set; }
        public double DzAltitude { get; set; }
        public DateTime ExitDateTime => Data.Find(d => d.Phase == FlightPhase.Freefall)?.Time ?? DateTime.MinValue;
        public double ExitAltitude => Data.Find(d => d.Phase == FlightPhase.Freefall)?.Altitude ?? 0;
        public double ExitHeight => ExitAltitude - DzAltitude;
        public DateTime PitchDateTime => Data.FindLast(d => d.Phase == FlightPhase.Freefall)?.Time ?? DateTime.MinValue;
        public double PitchAltitude => Data.FindLast(d => d.Phase == FlightPhase.Freefall)?.Altitude ?? 0;
        public double PitchHeight => PitchAltitude - DzAltitude;
        public DateTime LandingTime { get; set; }
        
        
        public List<TrackLog> Data { get; set; }

        // Summary values
        public double FreeFallTime
        {
            get
            {
                var lastFreefall = Data.FindLast(d => d.Phase == FlightPhase.Freefall);
                var firstFreefall = Data.Find(d => d.Phase == FlightPhase.Freefall);
                if (lastFreefall != null && firstFreefall != null)
                {
                    return (lastFreefall.Time - firstFreefall.Time).TotalSeconds;
                }
                return 0;
            }
        }
        public double VelocityTotalMax => Data.Where(d => d.Phase == FlightPhase.Freefall).Select(d => (double?)d.VelocityTotalKmh).Max() ?? 0;
        public double VelocityTotalMin => Data.Where(d => d.Phase == FlightPhase.Freefall).Select(d => (double?)d.VelocityTotalKmh).Min() ?? 0;
        public double VelocityGroundMax => Data.Where(d => d.Phase == FlightPhase.Freefall).Select(d => (double?)d.VelocityGroundKmh).Max() ?? 0;
        public double HorizontalDistance => Data.Where(d => d.Phase == FlightPhase.Freefall).Select(d => (double?)d.HorizontalDistance).Sum() ?? 0;
        public double GlideRatioMax => Data.Where(d => d.Phase == FlightPhase.Freefall).Select(d => (double?)d.GlideRatio).Max() ?? 0;

        // Constructor
        public Track()
        {
            Data = new List<TrackLog>();
        }

        /// <summary>
        /// Adjusts the start or end of the freefall trim.
        /// </summary>
        /// <param name="adjustStart">True to adjust the start, false to adjust the end.</param>
        /// <param name="seconds">The number of seconds to adjust. Positive values move the trim forward, negative values move it backward.</param>
        internal Task AdjustFreefallTrim(bool adjustStart, int seconds)
        {
            int index = adjustStart 
                ? Data.FindIndex(d => d.Phase == FlightPhase.Freefall) 
                : Data.FindLastIndex(d => d.Phase == FlightPhase.Freefall);

            if (index < 0)
            {
                Console.Error.WriteLine("No freefall phase found.");
                return Task.CompletedTask;
            }

            //Don't shorten freefall track if it's less than 10 seconds long
            if ((adjustStart && seconds > 0) || (!adjustStart && seconds < 0))
            {
                if (FreeFallTime < 10)
                {
                    Console.Error.WriteLine("Flight time is less than 10 seconds, cannot adjust freefall trim.");
                    return Task.CompletedTask;
                }
            }

            //Get time window to update
            DateTime referenceTime = Data[index].Time;
            DateTime minTime = seconds > 0 ? referenceTime : referenceTime.AddSeconds(seconds);
            DateTime maxTime = seconds > 0 ? referenceTime.AddSeconds(seconds) : referenceTime;

            var dataPoints = Data.Where(d => d.Time >= minTime && d.Time <= maxTime);
            foreach (var dataPoint in dataPoints)
            {
                dataPoint.Phase = adjustStart 
                    ? (seconds > 0 ? FlightPhase.Aircraft : FlightPhase.Freefall) 
                    : (seconds > 0 ? FlightPhase.Freefall : FlightPhase.Canopy);
            }

            Console.WriteLine($"Adjusted freefall {(adjustStart ? "start" : "end")} by {seconds} seconds. {dataPoints.Count()} data points affected.");
            return Task.CompletedTask;
        }
    }
}