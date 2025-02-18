using System;
using FlySightWebTool.Data;
using Xunit;

namespace FlySightWebTool.Tests
{
    public class TrackLogTests
    {
        [Fact]
        public void FromCsvLine_ValidLine_ReturnsTrackLog()
        {
            // Arrange
            string line = "$GNSS,2023-10-01T12:00:00Z,34.0000,-117.0000,1000,0,0,0,5,5,0.5,10";

            // Act
            var trackLog = TrackLog.FromCsvLine(line);

            // Assert
            Assert.NotNull(trackLog);
            Assert.Equal(new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc), trackLog.Time);
            Assert.Equal(34.0000, trackLog.Latitude);
            Assert.Equal(-117.0000, trackLog.Longitude);
            Assert.Equal(1000, trackLog.Altitude);
        }

        [Fact]
        public void FromCsvLine_InvalidLine_ThrowsFormatException()
        {
            // Arrange
            string line = "INVALID_LINE";

            // Act & Assert
            Assert.Throws<FormatException>(() => TrackLog.FromCsvLine(line));
        }

        [Fact]
        public void ComputeRelative_ValidPreviousTrackLog_ComputesCorrectValues()
        {
            // Arrange
            var prevTrackLog = new TrackLog
            {
                Time = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc),
                Altitude = 1000,
                VelocityNorth = 10,
                VelocityEast = 10,
                VelocityDown = -5
            };

            var trackLog = new TrackLog
            {
                Time = new DateTime(2023, 10, 1, 12, 0, 1, DateTimeKind.Utc),
                Altitude = 995,
                VelocityNorth = 15,
                VelocityEast = 15,
                VelocityDown = -10
            };

            double dzAltitude = 1000;
            DateTime exitDateTime = new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc);

            // Act
            trackLog.ComputeRelative(prevTrackLog, dzAltitude, exitDateTime);

            // Assert
            Assert.Equal(-5, trackLog.AccelerationDown, 1);
            Assert.Equal(5, trackLog.AccelerationEast, 1);
            Assert.Equal(5, trackLog.AccelerationNorth, 1);
            Assert.Equal(7.07, trackLog.AccelerationTotal, 2); // Approximate value
            Assert.Equal(7.07, trackLog.AccelerationGround, 2); // Approximate value
            Assert.Equal(995 - dzAltitude, trackLog.Height);
            Assert.Equal(1, trackLog.FlightTimeStamp);
        }
    }
}
