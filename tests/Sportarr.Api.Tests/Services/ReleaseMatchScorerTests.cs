using FluentAssertions;
using Sportarr.Api.Models;
using Sportarr.Api.Services;

namespace Sportarr.Api.Tests.Services;

public class ReleaseMatchScorerTests
{
    private readonly ReleaseMatchScorer _scorer = new();

    [Fact]
    public void CalculateMatchScore_ShouldNotHardRejectTeamSports_WhenRoundNumberDiffers()
    {
        // Arrange
        var evt = new Event
        {
            Title = "San Antonio Spurs vs Minnesota Timberwolves",
            Sport = "Basketball",
            League = new League { Name = "NBA", Sport = "Basketball" },
            Round = "3",
            EventDate = new DateTime(2026, 5, 8, 0, 0, 0, DateTimeKind.Utc),
            BroadcastDate = new DateTime(2026, 5, 8),
            HomeTeamName = "San Antonio Spurs",
            AwayTeamName = "Minnesota Timberwolves"
        };

        const string releaseTitle = "NBA Playoffs 2026 / Round 2 / Game 3 / 08.05.2026 / San Antonio Spurs @ Minnesota Timberwolves";

        // Act
        var score = _scorer.CalculateMatchScore(releaseTitle, evt);

        // Assert
        score.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ParseReleaseTitle_ShouldParseDdMmYyyyDateFormat()
    {
        // Arrange
        const string releaseTitle = "NBA Playoffs 2026 Round 2 Game 3 08.05.2026 San Antonio Spurs @ Minnesota Timberwolves";

        // Act
        var parsed = _scorer.ParseReleaseTitle(releaseTitle);

        // Assert
        parsed.Year.Should().Be(2026);
        parsed.Month.Should().Be(5);
        parsed.Day.Should().Be(8);
    }

    [Fact]
    public void ParseReleaseTitle_ShouldIgnoreInvalidDdMmYyyyMonthDay()
    {
        // Arrange
        const string releaseTitle = "NBA 2026 32.13.2026 San Antonio Spurs @ Minnesota Timberwolves";

        // Act
        var parsed = _scorer.ParseReleaseTitle(releaseTitle);

        // Assert
        parsed.Month.Should().BeNull();
        parsed.Day.Should().BeNull();
    }
}
