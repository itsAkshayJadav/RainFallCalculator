using RainfallCalculator.Console.Models;
using RainfallCalculator.Console.Services;

namespace RainFallCalculator.Tests;

public class RainfallAnalyzerTests
{
    private static readonly DateTime CurrentTime = new(2020, 6, 5, 14, 0, 0);
    private readonly RainfallAnalyzer _analyzer = new();

    [Fact]
    public void GetCurrentTime_ReturnsLatestReadingTime()
    {
        var readings = new List<RainfallReading>
        {
            CreateReading(10, 1m),
            CreateReading(14, 2m),
            CreateReading(12, 3m)
        };

        var currentTime = _analyzer.GetCurrentTime(readings);

        Assert.Equal(CurrentTime, currentTime);
    }

    [Fact]
    public void BuildSummaries_IgnoresReadingsOlderThanFourHours()
    {
        var summary = BuildSummary(
            CreateReading(9, 50m),
            CreateReading(10, 10m),
            CreateReading(14, 10m));

        Assert.Equal(10m, summary.AverageRainfallLast4Hours);
        Assert.Equal("Amber", summary.Status);
    }

    [Fact]
    public void BuildSummaries_ReturnsGreenWhenAverageIsBelowTen()
    {
        var summary = BuildSummary(
            CreateReading(10, 4m),
            CreateReading(12, 6m));

        Assert.Equal("Green", summary.Status);
    }

    [Fact]
    public void BuildSummaries_ReturnsRedWhenAverageIsAtLeastFifteen()
    {
        var summary = BuildSummary(
            CreateReading(10, 16m),
            CreateReading(12, 14m));

        Assert.Equal("Red", summary.Status);
    }

    [Fact]
    public void BuildSummaries_ReturnsRedWhenAnyReadingIsAboveThirty()
    {
        var summary = BuildSummary(
            CreateReading(10, 0m),
            CreateReading(11, 0m),
            CreateReading(12, 0m),
            CreateReading(13, 31m));

        Assert.Equal(7.75m, summary.AverageRainfallLast4Hours);
        Assert.Equal("Red", summary.Status);
    }

    [Fact]
    public void BuildSummaries_ReturnsIncreasingTrendWhenRecentReadingsAreHigher()
    {
        var summary = BuildSummary(
            CreateReading(10, 2m),
            CreateReading(11, 2m),
            CreateReading(12, 8m),
            CreateReading(13, 8m));

        Assert.Equal("Increasing", summary.Trend);
    }

    [Fact]
    public void BuildSummaries_ReturnsNoDataWhenOneTrendHalfHasNoReadings()
    {
        var summary = BuildSummary(
            CreateReading(12, 8m),
            CreateReading(13, 8m));

        Assert.Equal("No data", summary.Trend);
    }

    private DeviceSummary BuildSummary(params RainfallReading[] readings)
    {
        var summaries = _analyzer.BuildSummaries(
            new List<Device> { CreateDevice() },
            readings.ToList(),
            CurrentTime);

        return Assert.Single(summaries);
    }

    private static Device CreateDevice()
    {
        return new Device
        {
            DeviceId = "10451",
            DeviceName = "Gauge 1",
            Location = "Biyamiti"
        };
    }

    private static RainfallReading CreateReading(int hour, decimal rainfall)
    {
        return new RainfallReading
        {
            DeviceId = "10451",
            Time = new DateTime(2020, 6, 5, hour, 0, 0),
            Rainfall = rainfall
        };
    }
}
