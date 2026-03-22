using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainfallCalculator.Console.Models;

public class DeviceSummary
{
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public decimal AverageRainfallLast4Hours { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
}
