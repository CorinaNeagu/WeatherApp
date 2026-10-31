using System;
using System.Collections.Generic;
using System.Text;


namespace WeatherApp.Models;

public class DailyForecast
{
    public string Date { get; set; } = string.Empty;

    public double MinTemperature { get; set; }

    public double MaxTemperature { get; set; }
}
