using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RainfallCalculator.Console.Models;

public class RainfallReading
{
    public string DeviceId { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public decimal Rainfall { get; set; }
}
