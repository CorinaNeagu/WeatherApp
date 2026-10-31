using System;
using System.Collections.Generic;
using System.Text;


namespace WeatherApp.Models;

public class AirQualityData
{
    public string City { get; set; } = string.Empty;

    public double EuropeanAqi { get; set; }

    public double Pm25 { get; set; }

    public double Pm10 { get; set; }

    public double NitrogenDioxide { get; set; }

    public double Ozone { get; set; }
}
