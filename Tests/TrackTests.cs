using System;
using System.Collections.Generic;
using FlySightWebTool.Data;
using Xunit;

namespace FlySightWebTool.Tests
{
    public class TrackTests
    {
        [Fact]
        public void Track_Constructor_InitializesProperties()
        {
            // Arrange & Act
            var track = new Track();

            // Assert
            Assert.NotNull(track.Data);
            Assert.Empty(track.Data);
        }

        [Fact]
        public void Track_ExitHeight_CalculatesCorrectly()
        {
            // Arrange
            var track = new Track
            {
                DzAltitude = 1000,
                ExitAltitude = 2000
            };

            // Act
            var exitHeight = track.ExitHeight;

            // Assert
            Assert.Equal(1000, exitHeight);
        }

        [Fact]
        public void Track_PitchHeight_CalculatesCorrectly()
        {
            // Arrange
            var track = new Track
            {
                DzAltitude = 1000,
                PitchAltitude = 1500
            };

            // Act
            var pitchHeight = track.PitchHeight;

            // Assert
            Assert.Equal(500, pitchHeight);
        }

        [Fact]
        public void Track_HorizontalDistance_CalculatesCorrectly()
        {
            // Arrange
            var track = new Track
            {
                Data = new List<TrackLog>
                {
                    new TrackLog { HorizontalDistance = 100 },
                    new TrackLog { HorizontalDistance = 150 },
                    new TrackLog { HorizontalDistance = 200 }
                }
            };

            // Act
            var totalDistance = track.HorizontalDistance;

            // Assert
            Assert.Equal(450, totalDistance);
        }

        [Fact]
        public void Track_GlideRatioMax_CalculatesCorrectly()
        {
            // Arrange
            var track = new Track
            {
                Data = new List<TrackLog>
                {
                    new TrackLog { GlideRatio = 1.5 },
                    new TrackLog { GlideRatio = 2.0 },
                    new TrackLog { GlideRatio = 1.8 }
                }
            };

            // Act
            var maxGlideRatio = track.GlideRatioMax;

            // Assert
            Assert.Equal(2.0, maxGlideRatio);
        }
    }
}
