using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using WeatherApp.Models;

/*
namespace WeatherApp.Services
{
    internal class WeatherAPIService
    {
    }
}*/



namespace WeatherApp.Services;

public class WeatherAPIService
{
    private readonly HttpClient _httpClient;

    public WeatherAPIService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<WeatherData> GetCurrentWeatherAsync(
        string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException(
                "Introdu numele unui oraș.");

        // 1. Căutăm orașul și coordonatele sale.
        string cityUrl =
            $"https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={Uri.EscapeDataString(cityName)}" +
            "&count=1&language=ro&format=json";

        using var cityResponse =
            await _httpClient.GetAsync(cityUrl);

        cityResponse.EnsureSuccessStatusCode();

        using var cityJson = JsonDocument.Parse(
            await cityResponse.Content.ReadAsStringAsync());

        if (!cityJson.RootElement.TryGetProperty(
                "results", out var results) ||
            results.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                $"Nu am găsit orașul „{cityName}”.");
        }

        var city = results[0];

        string resolvedCityName =
            city.GetProperty("name").GetString() ?? cityName;

        string country =
            city.TryGetProperty("country", out var countryElement)
                ? countryElement.GetString() ?? string.Empty
                : string.Empty;

        double latitude =
            city.GetProperty("latitude").GetDouble();

        double longitude =
            city.GetProperty("longitude").GetDouble();

        // 2. Cerem vremea actuală pentru coordonatele găsite.
        string weatherUrl =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={latitude.ToString(
                System.Globalization.CultureInfo.InvariantCulture)}" +
            $"&longitude={longitude.ToString(
                System.Globalization.CultureInfo.InvariantCulture)}" +
            "&current=temperature_2m,relative_humidity_2m," +
            "apparent_temperature,wind_speed_10m,weather_code" +
            "&timezone=auto";

        using var weatherResponse =
            await _httpClient.GetAsync(weatherUrl);

        weatherResponse.EnsureSuccessStatusCode();

        using var weatherJson = JsonDocument.Parse(
            await weatherResponse.Content.ReadAsStringAsync());

        var current =
            weatherJson.RootElement.GetProperty("current");

        return new WeatherData
        {
            CityName = resolvedCityName,
            Country = country,
            Latitude = latitude,
            Longitude = longitude,

            Temperature = current
                .GetProperty("temperature_2m").GetDouble(),

            FeelsLike = current
                .GetProperty("apparent_temperature").GetDouble(),

            Humidity = current
                .GetProperty("relative_humidity_2m").GetInt32(),

            WindSpeed = current
                .GetProperty("wind_speed_10m").GetDouble(),

            WeatherCode = current
                .GetProperty("weather_code").GetInt32(),

            RetrievedAt = DateTime.Now
        };
    }
}
