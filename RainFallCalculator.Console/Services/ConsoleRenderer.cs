using RainfallCalculator.Console.Models;

namespace RainfallCalculator.Console.Services;

public class ConsoleRenderer
{
    public void Render(string folderPath, DateTime currentTime, List<DeviceSummary> summaries, List<string> warnings)
    {
        System.Console.WriteLine("Flood Detection Rainfall Summary");
        System.Console.WriteLine($"Folder: {Path.GetFullPath(folderPath)}");
        System.Console.WriteLine($"Assumed current time: {currentTime:yyyy-MM-dd HH:mm}");
        System.Console.WriteLine(new string('-', 95));
        System.Console.WriteLine(
            $"{"DeviceId",-10} {"DeviceName",-20} {"Location",-20} {"Avg Last 4h",-15} {"Status",-10} {"Trend",-12}");
        System.Console.WriteLine(new string('-', 95));

        foreach (var summary in summaries)
        {
            System.Console.WriteLine(
                $"{summary.DeviceId,-10} {summary.DeviceName,-20} {summary.Location,-20} {summary.AverageRainfallLast4Hours,-15:F2} {summary.Status,-10} {summary.Trend,-12}");
        }

        if (warnings.Count > 0)
        {
            System.Console.WriteLine();
            System.Console.WriteLine("Warnings:");
            foreach (var warning in warnings)
            {
                System.Console.WriteLine($" - {warning}");
            }
        }
    }
}
