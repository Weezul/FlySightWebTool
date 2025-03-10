using System.Security.Principal;

namespace FlySightWebTool.Data
{
    public class TrackService
    {
        private bool _hasTakenOff = false;
        private bool _hasExited = false;
        private bool _hasPitched = false;
        private bool _hasLanded = false;

        public Track Track { get; set; }

        private TrackLog? _prevTrackLog = null;
        private long _dzAltitudeCounter = 0;
        private double _dzAltitudeSum = 0.0;
        private FlightPhase currentPhase = FlightPhase.Boarding;

        public TrackService()
        {
            Track = new Track();
        }

        /// <summary>
        /// Loads track data from a file asynchronously.
        /// </summary>
        /// <param name="fileContent">The content of the file.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the loaded track.</returns>
        public Task<Track> LoadTrackFromFileAsync(string fileContent)
        {
            string[] lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                _prevTrackLog = AppendFromCsvLine(line);
            }

            validateTrack();

            return Task.FromResult(Track);
        }

        /// <summary>
        /// Appends a track log from a CSV line.
        /// </summary>
        /// <param name="line">The CSV line.</param>
        /// <returns>The appended track log.</returns>
        public TrackLog? AppendFromCsvLine(string line)
        {
            if (line.StartsWith("$GNSS"))
            {
                try
                {
                    var trackLog = TrackLog.FromCsvLine(line);
                    trackLog.Phase = currentPhase;

                    if (_prevTrackLog != null)
                    {
                        // Keep Height 0 until we have the DzAltitude
                        trackLog.ComputeRelative(_prevTrackLog,
                            Track.DzAltitude == 0.0 ? trackLog.Altitude : Track.DzAltitude,
                            Track.ExitDateTime);
                    }

                    // Calibrate DzAltitude until hasTakenOff
                    if (!_hasTakenOff && trackLog.AccuracyVertical < 10)
                    {
                        _dzAltitudeSum += trackLog.Altitude;
                        _dzAltitudeCounter++;
                    }

                    // Find takeOff
                    if (!_hasTakenOff &&
                        trackLog.AccuracyVertical < 5 &&
                        trackLog.VelocityDown < -2.5) // -10kmh
                    {
                        Track.DzAltitude = (_dzAltitudeSum / _dzAltitudeCounter);
                        Track.TakeOffDateTime = trackLog.Time;

                        _hasTakenOff = true;
                        currentPhase = FlightPhase.Aircraft;
                    }

                    // Free fall starts when downward speed is significantly negative
                    else if (_hasTakenOff &&
                            !_hasExited &&
                            trackLog.VelocityDown > 10 &&
                            trackLog.AccelerationDown > 3)
                    {
                        _hasExited = true;
                        currentPhase = FlightPhase.Freefall;
                    }
                    // During free fall
                    else if (_hasExited &&
                             !_hasPitched)
                    {
                        // Parachute opens when vertical speed reduces significantly                        
                        if ((trackLog.VelocityTotal - trackLog.VelocityDown) < 10 && 
                            trackLog.VelocityTotal < 80 &&
                            trackLog.AccelerationDown < -5 &&
                            trackLog.AccelerationTotal < -5)
                        {
                            _hasPitched = true;
                            currentPhase = FlightPhase.Canopy;
                        }
                    }
                    // Landing when speed is very low and altitude is near ground level
                    else if (_hasPitched &&
                            !_hasLanded &&
                            trackLog.VelocityDown < 1)
                    {
                        Track.LandingTime = trackLog.Time;

                        _hasLanded = true;
                        currentPhase = FlightPhase.Landed;
                    }

                    //Add to track
                    Track.Data.Add(trackLog);

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

        public void Reset()
        {
            _prevTrackLog = null;
            _dzAltitudeCounter = 0;
            _dzAltitudeSum = 0.0;
            _hasTakenOff = false;
            _hasExited = false;
            _hasPitched = false;
            _hasLanded = false;
            currentPhase = FlightPhase.Boarding;

            Track = new Track();
        }

        private void validateTrack()
        {
            if (Track.Data.Where(d => d.Phase == FlightPhase.Freefall).Count() == 0)
            {
                //Set all phases to Freefall
                foreach (var data in Track.Data)
                {
                    data.Phase = FlightPhase.Freefall;
                }    
            }
        }
    }
}
