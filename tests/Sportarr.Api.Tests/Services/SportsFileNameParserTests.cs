using Sportarr.Api.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Sportarr.Api.Tests.Services;

public class SportsFileNameParserTests
{
    private readonly SportsFileNameParser _parser;
    private readonly Mock<ILogger<SportsFileNameParser>> _mockLogger;

    public SportsFileNameParserTests()
    {
        _mockLogger = new Mock<ILogger<SportsFileNameParser>>();
        _parser = new SportsFileNameParser(_mockLogger.Object);
    }

    #region European Date Format Tests (DD.MM.YYYY)

    [Theory]
    [InlineData("NBA 2025-2026 RS 19.03.2026 Los Angeles Lakers @ Miami Heat Basketball WEB-DL HD 720p")]
    [InlineData("NBA.2025-2026.RS.19.03.2026.Los.Angeles.Lakers.@.Miami.Heat")]
    public void Parse_ShouldExtractEuropeanDate_AsEventDate(string filename)
    {
        // Act
        var result = _parser.Parse(filename);

        // Assert
        result.EventDate.Should().HaveValue();
        result.EventDate!.Value.Year.Should().Be(2026);
        result.EventDate!.Value.Month.Should().Be(3);
        result.EventDate!.Value.Day.Should().Be(19);
    }

    [Fact]
    public void Parse_ShouldExtractEuropeanDate_WithDashSeparator()
    {
        var result = _parser.Parse("NBA 2025-2026 RS 19-03-2026 Los Angeles Lakers @ Miami Heat");

        result.EventDate.Should().HaveValue();
        result.EventDate!.Value.Year.Should().Be(2026);
        result.EventDate!.Value.Month.Should().Be(3);
        result.EventDate!.Value.Day.Should().Be(19);
    }

    #endregion

    #region Year-First Date Format Tests (no regression)

    [Theory]
    [InlineData("NBA.2026.03.19.Los.Angeles.Lakers.vs.Miami.Heat", 2026, 3, 19)]
    [InlineData("NBA.2026-03-19.Los.Angeles.Lakers.vs.Miami.Heat", 2026, 3, 19)]
    public void Parse_ShouldExtractYearFirstDate_AsEventDate(string filename, int year, int month, int day)
    {
        // Act
        var result = _parser.Parse(filename);

        // Assert
        result.EventDate.Should().HaveValue();
        result.EventDate!.Value.Year.Should().Be(year);
        result.EventDate!.Value.Month.Should().Be(month);
        result.EventDate!.Value.Day.Should().Be(day);
    }

    #endregion
}
