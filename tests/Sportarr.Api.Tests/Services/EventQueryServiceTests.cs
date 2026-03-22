using Sportarr.Api.Services;
using Sportarr.Api.Models;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Sportarr.Api.Tests.Services;

/// <summary>
/// Tests for EventQueryService.BuildEventQueries, focused on team sport query generation.
/// </summary>
public class EventQueryServiceTests
{
    private readonly EventQueryService _service;

    public EventQueryServiceTests()
    {
        var logger = new Mock<ILogger<EventQueryService>>();
        _service = new EventQueryService(logger.Object);
    }

    private static Event MakeNbaEvent(
        string? homeTeamName = null,
        string? awayTeamName = null,
        Team? homeTeam = null,
        Team? awayTeam = null)
    {
        return new Event
        {
            Title = "NBA Test Event",
            Sport = "Basketball",
            EventDate = new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc),
            League = new League { Name = "NBA", Sport = "Basketball" },
            HomeTeamName = homeTeamName,
            AwayTeamName = awayTeamName,
            HomeTeam = homeTeam,
            AwayTeam = awayTeam,
        };
    }

    [Fact]
    public void BuildEventQueries_TeamSport_WithBothTeams_EmitsFourQueries()
    {
        var evt = MakeNbaEvent(homeTeamName: "Los Angeles Lakers", awayTeamName: "Miami Heat");

        var queries = _service.BuildEventQueries(evt);

        queries.Should().HaveCount(4);
        queries[0].Should().Be("NBA 2026 03 Lakers");
        queries[1].Should().Be("NBA 2026 03 Heat");
        queries[2].Should().Be("NBA 2026 03");
        queries[3].Should().Be("NBA 2026");
    }

    [Fact]
    public void BuildEventQueries_TeamSport_HomeTeamOnly_EmitsThreeQueries()
    {
        var evt = MakeNbaEvent(homeTeamName: "Golden State Warriors");

        var queries = _service.BuildEventQueries(evt);

        queries.Should().HaveCount(3);
        queries[0].Should().Be("NBA 2026 03 Warriors");
        queries[1].Should().Be("NBA 2026 03");
        queries[2].Should().Be("NBA 2026");
    }

    [Fact]
    public void BuildEventQueries_TeamSport_NoTeams_EmitsTwoQueries()
    {
        var evt = MakeNbaEvent();

        var queries = _service.BuildEventQueries(evt);

        queries.Should().HaveCount(2);
        queries[0].Should().Be("NBA 2026 03");
        queries[1].Should().Be("NBA 2026");
    }

    [Fact]
    public void BuildEventQueries_TeamSport_NavigationTeamPreferredOverStringField()
    {
        var evt = MakeNbaEvent(
            homeTeamName: "Old Name Lakers",
            awayTeamName: "Old Name Heat",
            homeTeam: new Team { Name = "Oklahoma City Thunder" },
            awayTeam: new Team { Name = "Dallas Mavericks" });

        var queries = _service.BuildEventQueries(evt);

        queries.Should().Contain("NBA 2026 03 Thunder");
        queries.Should().Contain("NBA 2026 03 Mavericks");
        queries.Should().NotContain(q => q.Contains("Lakers") || q.Contains("Heat"));
    }

    [Fact]
    public void BuildEventQueries_TeamSport_SameNickname_Deduplicated()
    {
        // Both teams end in "United" (e.g. "Manchester United" vs "Leeds United")
        var evt = new Event
        {
            Title = "MLS Test",
            Sport = "Soccer",
            EventDate = new DateTime(2026, 3, 19, 0, 0, 0, DateTimeKind.Utc),
            League = new League { Name = "MLS", Sport = "Soccer" },
            HomeTeamName = "Atlanta United",
            AwayTeamName = "New England United",
        };

        var queries = _service.BuildEventQueries(evt);

        // "United" appears only once in team queries
        queries.Count(q => q.Contains("United")).Should().Be(1);
        queries.Should().HaveCount(3); // home-nickname, date, year
    }

    [Fact]
    public void BuildEventQueries_TeamSport_SingleWordTeamName_ExtractsCorrectly()
    {
        var evt = MakeNbaEvent(homeTeamName: "Heat", awayTeamName: "Jazz");

        var queries = _service.BuildEventQueries(evt);

        queries[0].Should().Be("NBA 2026 03 Heat");
        queries[1].Should().Be("NBA 2026 03 Jazz");
    }
}
