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

    public async Task<List<DailyForecast>> GetForecastAsync(
        string cityName,
        int days)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("Introdu numele unui oraș.");

        if (days != 3 && days != 5 && days != 7 && days != 14)
            throw new ArgumentException("Alege 3, 5, 7 sau 14 zile.");

        string cityUrl =
            "https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={Uri.EscapeDataString(cityName)}" +
            "&count=1&language=ro&format=json";

        using var cityResponse = await _httpClient.GetAsync(cityUrl);
        cityResponse.EnsureSuccessStatusCode();

        using var cityJson = JsonDocument.Parse(
            await cityResponse.Content.ReadAsStringAsync());

        if (!cityJson.RootElement.TryGetProperty("results", out var results)
            || results.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                $"Nu am găsit orașul „{cityName}”.");
        }

        var city = results[0];

        double latitude = city.GetProperty("latitude").GetDouble();
        double longitude = city.GetProperty("longitude").GetDouble();

        string lat = latitude.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        string lon = longitude.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        string weatherUrl =
            "https://api.open-meteo.com/v1/forecast" +
            $"?latitude={lat}&longitude={lon}" +
            $"&daily=temperature_2m_max,temperature_2m_min" +
            $"&forecast_days={days}&timezone=auto";

        using var weatherResponse = await _httpClient.GetAsync(weatherUrl);
        weatherResponse.EnsureSuccessStatusCode();

        using var weatherJson = JsonDocument.Parse(
            await weatherResponse.Content.ReadAsStringAsync());

        var daily = weatherJson.RootElement.GetProperty("daily");

        var dates = daily.GetProperty("time");
        var minimums = daily.GetProperty("temperature_2m_min");
        var maximums = daily.GetProperty("temperature_2m_max");

        var forecast = new List<DailyForecast>();

        for (int i = 0; i < dates.GetArrayLength(); i++)
        {
            forecast.Add(new DailyForecast
            {
                Date = dates[i].GetString() ?? string.Empty,
                MinTemperature = minimums[i].GetDouble(),
                MaxTemperature = maximums[i].GetDouble()
            });
        }

        return forecast;
    }

    public async Task<AirQualityData> GetAirQualityAsync(string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            throw new ArgumentException("Introdu numele unui oraș.");

        string cityUrl =
            "https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={Uri.EscapeDataString(cityName)}" +
            "&count=1&language=ro&format=json";

        using var cityResponse = await _httpClient.GetAsync(cityUrl);
        cityResponse.EnsureSuccessStatusCode();

        using var cityJson = JsonDocument.Parse(
            await cityResponse.Content.ReadAsStringAsync());

        if (!cityJson.RootElement.TryGetProperty("results", out var results)
            || results.GetArrayLength() == 0)
        {
            throw new InvalidOperationException(
                $"Nu am găsit orașul „{cityName}”.");
        }

        var city = results[0];

        double latitude = city.GetProperty("latitude").GetDouble();
        double longitude = city.GetProperty("longitude").GetDouble();

        string lat = latitude.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        string lon = longitude.ToString(
            System.Globalization.CultureInfo.InvariantCulture);

        string airQualityUrl =
            "https://air-quality-api.open-meteo.com/v1/air-quality" +
            $"?latitude={lat}&longitude={lon}" +
            "&current=european_aqi,pm2_5,pm10,nitrogen_dioxide,ozone" +
            "&timezone=auto";

        using var airResponse = await _httpClient.GetAsync(airQualityUrl);
        airResponse.EnsureSuccessStatusCode();

        using var airJson = JsonDocument.Parse(
            await airResponse.Content.ReadAsStringAsync());

        var current = airJson.RootElement.GetProperty("current");

        return new AirQualityData
        {
            City = city.TryGetProperty("name", out var name)
                ? name.GetString() ?? cityName
                : cityName,

            EuropeanAqi = current.GetProperty("european_aqi").GetDouble(),
            Pm25 = current.GetProperty("pm2_5").GetDouble(),
            Pm10 = current.GetProperty("pm10").GetDouble(),
            NitrogenDioxide =
                current.GetProperty("nitrogen_dioxide").GetDouble(),
            Ozone = current.GetProperty("ozone").GetDouble()
        };
    }

}
