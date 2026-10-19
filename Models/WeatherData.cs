using System;
using System.Collections.Generic;
using System.Text;

/*
namespace WeatherApp.Models

internal class WeatherData
{
}

*/

namespace WeatherApp.Models;

public class WeatherData
{
    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double Temperature { get; set; }
    public double FeelsLike { get; set; }

    public int Humidity { get; set; }
    public double WindSpeed { get; set; }

    public int WeatherCode { get; set; }
    public DateTime RetrievedAt { get; set; }
}
