namespace FlySightWebTool.Data;

public class TrackService
{
    bool hasTakenOff = false;
    bool hasExited = false;
    bool hasPitched = false;
    bool hasLanded = false;

    public Track Track { get; set; }

    private TrackLog? prevTrackLog = null;
    private long dzAltitudeCounter = 0;
    private double dzAltitudeSum = 0.0;

    public TrackService()
    {
        Track = new Track();
    }

    public Task<Track> LoadTrackFromFileAsync(string fileContent)
    {
        string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            prevTrackLog = AppendFromCsvLine(line);
        }

        return Task.FromResult(Track);
    }

    public TrackLog? AppendFromCsvLine(string line)
    {
        if (line.StartsWith("$GNSS"))
        {
            try
            {
                var trackLog = TrackLog.FromCsvLine(line);

                if (prevTrackLog != null)
                {
                    //Keep Height 0 until we have the DzAltitude
                    trackLog.computeRelative(prevTrackLog,
                        Track.DzAltitude == 0.0 ? trackLog.Altitude : Track.DzAltitude,
                        Track.ExitDateTime);
                }

                //Calibrate DzAltitude until hasTakenOff
                if (!hasTakenOff && trackLog.AccuracyVertical < 10)
                {
                    dzAltitudeSum += trackLog.Altitude;
                    dzAltitudeCounter++;
                }

                //Check for takeOff
                if (!hasTakenOff &&
                    trackLog.VelocityDown < -2.5) //-10kmh
                {
                    Track.DzAltitude = (dzAltitudeSum / dzAltitudeCounter);
                    Track.TakeOffDateTime = trackLog.Time;

                    hasTakenOff = true;
                }

                //Free fall starts when downward speed is significantly negative
                else if (hasTakenOff &&
                        !hasExited &&
                        trackLog.VelocityDown > 20) //Should be acceleration down over 3!! accoridng to flysight :)
                {
                    Track.ExitDateTime = trackLog.Time;
                    Track.ExitAltitude = trackLog.Altitude;
                    hasExited = true;
                }
                //During free fall
                else if (hasExited &&
                         !hasPitched)
                {
                    Track.Data.Add(trackLog);

                    //Parachute opens when vertical speed reduces significantly
                    if (trackLog.VelocityDown < 5)
                    {
                        Track.PitchDateTime = trackLog.Time;
                        Track.PitchAltitude = trackLog.Altitude;
                        Track.FlightTime = trackLog.FlightTimeStamp;
                        hasPitched = true;
                    }
                }
                //Landing when speed is very low and altitude is near ground level
                else if (hasPitched &&
                        !hasLanded &&
                        trackLog.VelocityDown < 1)
                {
                    Track.LandingTime = trackLog.Time;
                    hasLanded = true;
                }

                return trackLog;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing line: '{line}', {ex.Message}");
            }
        }

        Console.WriteLine($"Skipping non-DATA record: '{line}'");
        return null;
    } 
}
