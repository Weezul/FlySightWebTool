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
        public DateTime ExitDateTime { get; set; }
        public double ExitAltitude { get; set; }
        public double ExitHeight => ExitAltitude - DzAltitude;
        public DateTime PitchDateTime { get; set; }
        public double PitchAltitude { get; set; }
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

        internal Task AdjustFreefallTrimStart(int seconds)
        {
            var firstFreefallIndex = Data.FindIndex(d => d.Phase == FlightPhase.Freefall);
            if (firstFreefallIndex >= 0)
            {
                if (seconds > 0)
                {
                    //Move trim forward (Shorter flight)
                    if (FreeFallTime < 10)
                    {
                        Console.Error.WriteLine("Flight time is less than 10 seconds, cannot adjust freefall start.");
                        return Task.CompletedTask;
                    }

                    //Select all data points where time is between first freefall index time and first freefall index time + seconds
                    var dataPoints = Data.Where(d => d.Time >= Data[firstFreefallIndex].Time && d.Time <= Data[firstFreefallIndex].Time.AddSeconds(seconds));

                    foreach (var dataPoint in dataPoints)
                    {
                        dataPoint.Phase = FlightPhase.Aircraft;
                    }
                    Console.WriteLine($"Adjusted freefall start by {seconds} seconds. {dataPoints.Count()} data points affected.");
                }
                else
                {
                    //Move trim backward (Longer flight)
                    //Select all data points where time is between first freefall index time and first freefall index time - seconds
                    var dataPoints = Data.Where(d => d.Time >= Data[firstFreefallIndex].Time.AddSeconds(seconds) && d.Time <= Data[firstFreefallIndex].Time);
                    foreach (var dataPoint in dataPoints)
                    {
                        dataPoint.Phase = FlightPhase.Freefall;
                    }
                    Console.WriteLine($"Adjusted freefall start by {seconds} seconds. {dataPoints.Count()} data points affected.");
                }                    
            }

            return Task.CompletedTask;
        }

        internal Task AdjustFreefallTrimEnd(int seconds)
        {
            var lastFreefallIndex = Data.FindLastIndex(d => d.Phase == FlightPhase.Freefall);
            if (lastFreefallIndex >= 0)
            {
                if (seconds > 0)
                {
                    //Select all data points where time is between last freefall index time and last freefall index time + seconds
                    var dataPoints = Data.Where(d => d.Time >= Data[lastFreefallIndex].Time && d.Time <= Data[lastFreefallIndex].Time.AddSeconds(seconds));

                    foreach (var dataPoint in dataPoints)
                    {
                        dataPoint.Phase = FlightPhase.Freefall;
                    }
                    Console.WriteLine($"Adjusted freefall end by {seconds} seconds. {dataPoints.Count()} data points affected.");
                }
                else
                {
                    if (FreeFallTime < 10)
                    {
                        Console.Error.WriteLine("Flight time is less than 10 seconds, cannot adjust freefall end.");
                        return Task.CompletedTask;
                    }

                    //Select all data points where time is between last freefall index time and last freefall index time - seconds
                    var dataPoints = Data.Where(d => d.Time >= Data[lastFreefallIndex].Time.AddSeconds(seconds) && d.Time <= Data[lastFreefallIndex].Time);
                    foreach (var dataPoint in dataPoints)
                    {
                        dataPoint.Phase = FlightPhase.Canopy;
                    }
                    Console.WriteLine($"Adjusted freefall end by {seconds} seconds. {dataPoints.Count()} data points affected.");
                }                    
            }

            return Task.CompletedTask;
        }
    }
}