using RainfallCalculator.Console.Models;

namespace RainfallCalculator.Console.Services;

public class RainfallAnalyzer
{
    private const decimal GreenThreshold = 10m;
    private const decimal RedThreshold = 15m;
    private const decimal InstantRedThreshold = 30m;

    public DateTime GetCurrentTime(List<RainfallReading> readings)
    {
        if (readings.Count == 0)
        {
            throw new InvalidOperationException("No valid rainfall readings were loaded.");
        }

        return readings.Max(reading => reading.Time);
    }

    public List<DeviceSummary> BuildSummaries(List<Device> devices, List<RainfallReading> readings, DateTime currentTime)
    {
        var startTime = currentTime.AddHours(-4);
        var trendSplitTime = currentTime.AddHours(-2);
        var summaries = new List<DeviceSummary>();

        foreach (var device in devices.OrderBy(device => device.DeviceId, StringComparer.OrdinalIgnoreCase))
        {
            var recentReadings = readings
                .Where(reading => reading.DeviceId == device.DeviceId)
                .Where(reading => reading.Time >= startTime && reading.Time <= currentTime)
                .OrderBy(reading => reading.Time)
                .ToList();

            var averageRainfall = recentReadings.Count == 0
                ? 0m
                : Math.Round(recentReadings.Average(reading => reading.Rainfall), 2);

            var olderReadings = recentReadings
                .Where(reading => reading.Time < trendSplitTime)
                .ToList();

            var newerReadings = recentReadings
                .Where(reading => reading.Time >= trendSplitTime)
                .ToList();

            summaries.Add(new DeviceSummary
            {
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                Location = device.Location,
                AverageRainfallLast4Hours = averageRainfall,
                Status = GetStatus(recentReadings, averageRainfall),
                Trend = GetTrend(olderReadings, newerReadings)
            });
        }

        return summaries;
    }

    private static string GetStatus(List<RainfallReading> recentReadings, decimal averageRainfall)
    {
        if (recentReadings.Any(reading => reading.Rainfall > InstantRedThreshold) || averageRainfall >= RedThreshold)
        {
            return "Red";
        }

        if (averageRainfall < GreenThreshold)
        {
            return "Green";
        }

        return "Amber";
    }

    private static string GetTrend(List<RainfallReading> olderReadings, List<RainfallReading> newerReadings)
    {
        if (olderReadings.Count == 0 || newerReadings.Count == 0)
        {
            return "No data";
        }

        var olderAverage = olderReadings.Average(reading => reading.Rainfall);
        var newerAverage = newerReadings.Average(reading => reading.Rainfall);

        if (newerAverage > olderAverage)
        {
            return "Increasing";
        }

        if (newerAverage < olderAverage)
        {
            return "Decreasing";
        }

        return "Stable";
    }
}
