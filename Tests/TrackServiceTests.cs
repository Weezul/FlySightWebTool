using System;
using System.Threading.Tasks;
using FlySightWebTool.Data;
using Xunit;

namespace FlySightWebTool.Tests
{
    public class TrackServiceTests
    {
        [Fact]
        public async Task LoadTrackFromFileAsync_ValidFileContent_ReturnsTrack()
        {
            // Arrange
            var trackService = new TrackService();
            string fileContent = "$GNSS,2023-10-01T12:00:00Z,34.0000,-117.0000,1000,0,0,0,5,5,0.5,10\n" +
                                 "$GNSS,2023-10-01T12:00:01Z,34.0001,-117.0001,1005,0,0,0,5,5,0.5,10\n";

            // Act
            var track = await trackService.LoadTrackFromFileAsync(fileContent);

            // Assert
            Assert.NotNull(track);
            Assert.Equal(2, track.Data.Count);
            Assert.Equal(new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc), track.Data[0].Time);
            Assert.Equal(34.0000, track.Data[0].Latitude);
            Assert.Equal(-117.0000, track.Data[0].Longitude);
            Assert.Equal(1000, track.Data[0].Altitude);
        }

        [Fact]
        public void AppendFromCsvLine_ValidLine_ReturnsTrackLog()
        {
            // Arrange
            var trackService = new TrackService();
            string line = "$GNSS,2023-10-01T12:00:00Z,34.0000,-117.0000,1000,0,0,0,5,5,0.5,10";

            // Act
            var trackLog = trackService.AppendFromCsvLine(line);

            // Assert
            Assert.NotNull(trackLog);
            Assert.Equal(new DateTime(2023, 10, 1, 12, 0, 0, DateTimeKind.Utc), trackLog.Time);
            Assert.Equal(34.0000, trackLog.Latitude);
            Assert.Equal(-117.0000, trackLog.Longitude);
            Assert.Equal(1000, trackLog.Altitude);
        }

        [Fact]
        public void AppendFromCsvLine_InvalidLine_ThrowsException()
        {
            // Arrange
            var trackService = new TrackService();
            string line = "INVALID_LINE";

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => trackService.AppendFromCsvLine(line));
            Assert.Contains("Error parsing line", exception.Message);
        }
    }
}
