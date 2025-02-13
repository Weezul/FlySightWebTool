using System;
using System.Linq;
using System.Collections.Generic;

namespace FlySightWebTool.Data;

public class Track
{
    public DateTime TakeOffDateTime { get; set; }
    public double DzAltitude { get; set; }
    public DateTime ExitDateTime { get; set; }
    public double ExitAltitude { get; set; }
    public double ExitHeight => ExitAltitude - DzAltitude;
    public DateTime PitchDateTime { get; set; }
    public double PitchAltitude { get; set; }
    public double PitchHeight => PitchAltitude - DzAltitude;
    public DateTime LandingTime { get; set; }
    public double FlightTime { get; set; }
    
    public List<TrackLog> Data { get; set; }

    //Max values
    public double VelocityTotalMax => Data.Max(d => d.VelocityTotalKmh);
    public double VelocityTotalMin => Data.Min(d => d.VelocityTotalKmh);
    public double VelocityGroundMax => Data.Max(d => d.VelocityGroundKmh);
    public double GlideRatioMax => Data.Max(d => d.GlideRatio);    

    public Track()
    {
        Data = new List<TrackLog>();
    }
}