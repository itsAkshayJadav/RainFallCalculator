using RainfallCalculator.Console.Services;

namespace RainfallCalculator.Console;

internal class Program
{
    private static int Main(string[] args)
    {
        try
        {
            string? folderPath;

            if (args.Length > 0)
            {
                folderPath = args[0];
            }
            else
            {
                System.Console.Write("Enter data folder: ");
                folderPath = System.Console.ReadLine();
            }

            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                System.Console.WriteLine("Invalid folder path.");
                return 1;
            }

            var warnings = new List<string>();
            var loader = new CsvLoader();
            var devices = loader.LoadDevices(folderPath, warnings);
            var readings = loader.LoadRainfallReadings(folderPath, devices, warnings);

            if (devices.Count == 0)
            {
                System.Console.WriteLine("No valid devices were loaded.");
                return 1;
            }

            var analyzer = new RainfallAnalyzer();
            var currentTime = analyzer.GetCurrentTime(readings);
            var summaries = analyzer.BuildSummaries(devices, readings, currentTime);

            var renderer = new ConsoleRenderer();
            renderer.Render(folderPath, currentTime, summaries, warnings);

            return 0;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }
}
