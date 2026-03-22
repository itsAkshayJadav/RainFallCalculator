using RainfallCalculator.Console.Models;
using RainfallCalculator.Console.Services;

namespace RainFallCalculator.Tests;

public class CsvLoaderTests
{
    private readonly CsvLoader _loader = new();

    [Fact]
    public void LoadDevices_IgnoresDuplicateDeviceIdsAndAddsWarning()
    {
        var folderPath = CreateTestFolder();

        try
        {
            File.WriteAllText(
                Path.Combine(folderPath, "Devices.csv"),
                """
                Device ID,Device Name,Location
                10451,Gauge 1,Biyamiti
                10451,Gauge 2,Balule
                """);

            var warnings = new List<string>();

            var devices = _loader.LoadDevices(folderPath, warnings);

            var device = Assert.Single(devices);
            Assert.Equal("10451", device.DeviceId);
            Assert.Contains(warnings, warning => warning.Contains("duplicate device id '10451'"));
        }
        finally
        {
            DeleteTestFolder(folderPath);
        }
    }

    [Fact]
    public void LoadRainfallReadings_SkipsInvalidRowsAndReturnsOnlyValidReadings()
    {
        var folderPath = CreateTestFolder();

        try
        {
            File.WriteAllText(
                Path.Combine(folderPath, "Devices.csv"),
                """
                Device ID,Device Name,Location
                10451,Gauge 1,Biyamiti
                """);

            File.WriteAllText(
                Path.Combine(folderPath, "Data1.csv"),
                """
                Device ID,Time,Rainfall
                10451,05/06/2020 10:00,5
                99999,05/06/2020 10:30,5
                10451,not-a-date,4
                10451,05/06/3030 11:30,52
                10451,05/06/2020 12:00,2k4
                10451,05/06/2020 12:30,-1
                """);

            var warnings = new List<string>();
            var devices = _loader.LoadDevices(folderPath, warnings);

            var readings = _loader.LoadRainfallReadings(folderPath, devices, warnings);

            var reading = Assert.Single(readings);
            Assert.Equal("10451", reading.DeviceId);
            Assert.Equal(new DateTime(2020, 6, 5, 10, 0, 0), reading.Time);
            Assert.Equal(5m, reading.Rainfall);
            Assert.Contains(warnings, warning => warning.Contains("unknown device id '99999'"));
            Assert.Contains(warnings, warning => warning.Contains("invalid timestamp 'not-a-date'"));
            Assert.Contains(warnings, warning => warning.Contains("too far in the future"));
            Assert.Contains(warnings, warning => warning.Contains("invalid rainfall value '2k4'"));
            Assert.Contains(warnings, warning => warning.Contains("negative rainfall ignored"));
        }
        finally
        {
            DeleteTestFolder(folderPath);
        }
    }

    private static string CreateTestFolder()
    {
        var folderPath = Path.Combine(AppContext.BaseDirectory, "TestData", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(folderPath);
        return folderPath;
    }

    private static void DeleteTestFolder(string folderPath)
    {
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);
        }
    }
}
