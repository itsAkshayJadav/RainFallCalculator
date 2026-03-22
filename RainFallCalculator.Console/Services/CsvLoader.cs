using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using RainfallCalculator.Console.Models;

namespace RainfallCalculator.Console.Services;

public class CsvLoader
{
    private static readonly string[] TimestampFormats =
    {
        "dd/MM/yyyy H:mm",
        "dd/MM/yyyy HH:mm"
    };

    public List<Device> LoadDevices(string folderPath, List<string> warnings)
    {
        var devicesFile = Path.Combine(folderPath, "Devices.csv");
        if (!File.Exists(devicesFile))
        {
            throw new FileNotFoundException("Devices.csv was not found in the selected folder.");
        }

        var devices = new List<Device>();
        var knownIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var reader = new StreamReader(devicesFile);
        using var csv = new CsvReader(reader, CreateConfiguration());

        csv.Read();
        csv.ReadHeader();
        ValidateHeaders(csv.HeaderRecord, devicesFile, "Device ID", "Device Name", "Location");

        while (csv.Read())
        {
            var rowNumber = csv.Context.Parser?.RawRow ?? 0;
            var deviceId = GetField(csv, "Device ID");
            var deviceName = GetField(csv, "Device Name");
            var location = GetField(csv, "Location");

            if (string.IsNullOrWhiteSpace(deviceId) ||
                string.IsNullOrWhiteSpace(deviceName) ||
                string.IsNullOrWhiteSpace(location))
            {
                warnings.Add($"Devices.csv line {rowNumber}: missing device details.");
                continue;
            }

            if (!knownIds.Add(deviceId))
            {
                warnings.Add($"Devices.csv line {rowNumber}: duplicate device id '{deviceId}' ignored.");
                continue;
            }

            devices.Add(new Device
            {
                DeviceId = deviceId,
                DeviceName = deviceName,
                Location = location
            });
        }

        return devices;
    }

    public List<RainfallReading> LoadRainfallReadings(string folderPath, List<Device> devices, List<string> warnings)
    {
        var deviceIds = devices
            .Select(device => device.DeviceId)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var rainfallFiles = Directory
            .GetFiles(folderPath, "*.csv")
            .Where(file => !string.Equals(Path.GetFileName(file), "Devices.csv", StringComparison.OrdinalIgnoreCase))
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (rainfallFiles.Count == 0)
        {
            throw new FileNotFoundException("No rainfall CSV files were found in the selected folder.");
        }

        var readings = new List<RainfallReading>();

        foreach (var rainfallFile in rainfallFiles)
        {
            var fileName = Path.GetFileName(rainfallFile);

            using var reader = new StreamReader(rainfallFile);
            using var csv = new CsvReader(reader, CreateConfiguration());

            csv.Read();
            csv.ReadHeader();
            ValidateHeaders(csv.HeaderRecord, rainfallFile, "Device ID", "Time", "Rainfall");

            while (csv.Read())
            {
                var rowNumber = csv.Context.Parser?.RawRow ?? 0;
                var deviceId = GetField(csv, "Device ID");
                var timeText = GetField(csv, "Time");
                var rainfallText = GetField(csv, "Rainfall");

                if (!deviceIds.Contains(deviceId))
                {
                    warnings.Add($"{fileName} line {rowNumber}: unknown device id '{deviceId}' ignored.");
                    continue;
                }

                if (!DateTime.TryParseExact(timeText, TimestampFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
                {
                    warnings.Add($"{fileName} line {rowNumber}: invalid timestamp '{timeText}'.");
                    continue;
                }

                if (timestamp.Year > 2100)
                {
                    warnings.Add($"{fileName} line {rowNumber}: timestamp '{timeText}' is too far in the future and was ignored.");
                    continue;
                }

                if (!decimal.TryParse(rainfallText, NumberStyles.Number, CultureInfo.InvariantCulture, out var rainfall))
                {
                    warnings.Add($"{fileName} line {rowNumber}: invalid rainfall value '{rainfallText}'.");
                    continue;
                }

                if (rainfall < 0)
                {
                    warnings.Add($"{fileName} line {rowNumber}: negative rainfall ignored.");
                    continue;
                }

                readings.Add(new RainfallReading
                {
                    DeviceId = deviceId,
                    Time = timestamp,
                    Rainfall = rainfall
                });
            }
        }

        return readings;
    }

    private static CsvConfiguration CreateConfiguration()
    {
        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
            MissingFieldFound = null,
            HeaderValidated = null,
            BadDataFound = null
        };
    }

    private static string GetField(CsvReader csv, string columnName)
    {
        return csv.GetField(columnName)?.Trim() ?? string.Empty;
    }

    private static void ValidateHeaders(string[]? headers, string filePath, params string[] expectedHeaders)
    {
        if (headers is null)
        {
            throw new InvalidDataException($"{Path.GetFileName(filePath)} does not contain a header row.");
        }

        foreach (var expectedHeader in expectedHeaders)
        {
            var found = headers.Any(header =>
                string.Equals(header?.Trim(), expectedHeader, StringComparison.OrdinalIgnoreCase));

            if (!found)
            {
                throw new InvalidDataException($"{Path.GetFileName(filePath)} is missing required column '{expectedHeader}'.");
            }
        }
    }
}
