using System;

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

    public Track()
    {
        Data = new List<TrackLog>();
    }
}