using System;
using System.Collections.Generic;
using System.Text;


/*
namespace WeatherApp.Models
{
    internal class WeatherSearchHistory
    {
    }
}
*/



namespace WeatherApp.Models;

public class WeatherSearchHistory
{
    public int Id { get; set; }

    public string CityName { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public double Temperature { get; set; }
    public double FeelsLike { get; set; }

    public int Humidity { get; set; }
    public double WindSpeed { get; set; }

    public int WeatherCode { get; set; }
    public DateTime SearchedAt { get; set; }
}
