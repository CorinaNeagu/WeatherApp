using System;
using System.Collections.Generic;
using System.Text;

namespace WeatherApp.Models;

public class FavoriteCity
{
    public int Id { get; set; }
    public string CityName { get; set; } = "";
    public string Country { get; set; } = "";
}

