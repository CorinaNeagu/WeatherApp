using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using WeatherApp.Models;
using WeatherApp.Repositories;

namespace WeatherApp.Services;

public class WeatherStatisticsService
{
    private readonly HttpClient _httpClient;
    private readonly WeatherStatisticsRepository _repository;

    public WeatherStatisticsService(
        HttpClient httpClient,
        WeatherStatisticsRepository repository)
    {
        _httpClient = httpClient;
        _repository = repository;
    }
    public async Task<List<CitySearchResult>> SearchCitiesAsync(
        string cityName)
    {
        if (string.IsNullOrWhiteSpace(cityName))
            return new List<CitySearchResult>();

        string url =
            "https://geocoding-api.open-meteo.com/v1/search" +
            $"?name={Uri.EscapeDataString(cityName)}" +
            "&count=10&language=ro&format=json";

        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result = JsonSerializer.Deserialize<GeocodingResponse>(json);

        return result?.Results?
            .Select(x => new CitySearchResult
            {
                Name = x.Name ?? "",
                Country = x.Country ?? "",
                Latitude = x.Latitude,
                Longitude = x.Longitude
            })
            .ToList() ?? new List<CitySearchResult>();
    }



    public async Task<List<CityWeatherStatistics>>
        GetHistoricalStatisticsAsync(
            CitySearchResult city,
            int days)
    {
        return await GetHistoricalStatisticsAsync(
            city,
            days,
            DateTime.Today.AddDays(-1));
    }

    public async Task<List<CityWeatherStatistics>>
        GetHistoricalStatisticsAsync(
            CitySearchResult city,
            int days,
            DateTime endDate)
    {
        if (days != 7 && days != 30 &&
            days != 90 && days != 180 &&
            days != 270 && days != 365)
        {
            throw new ArgumentException(
                "Intervalul selectat nu este valid.");
        }

        endDate = endDate.Date;

        DateTime startDate = days switch
        {
            7 => endDate.AddDays(-6),
            30 => endDate.AddDays(-29),
            90 => endDate.AddMonths(-3).AddDays(1),
            180 => endDate.AddMonths(-6).AddDays(1),
            270 => endDate.AddMonths(-9).AddDays(1),
            365 => endDate.AddYears(-1).AddDays(1),
            _ => endDate.AddDays(-6)
        };


        string url =
            "https://archive-api.open-meteo.com/v1/archive" +
            $"?latitude={city.Latitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&longitude={city.Longitude.ToString(CultureInfo.InvariantCulture)}" +
            $"&start_date={startDate:yyyy-MM-dd}" +
            $"&end_date={endDate:yyyy-MM-dd}" +
            "&daily=temperature_2m_mean" +
            ",apparent_temperature_mean" +
            ",relative_humidity_2m_mean" +
            ",wind_speed_10m_mean" +
            ",weather_code" +
            "&timezone=auto";

        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var result =
            JsonSerializer.Deserialize<ArchiveResponse>(json);

        var statistics = new List<CityWeatherStatistics>();

        if (result?.Daily?.Time == null)
            return statistics;

        for (int i = 0; i < result.Daily.Time.Count; i++)
        {
            var temperature = result.Daily.Temperature?[i];

            if (temperature == null)
                continue;

            var item = new CityWeatherStatistics
            {
                CityName = city.Name,
                Country = city.Country,
                Latitude = city.Latitude,
                Longitude = city.Longitude,
                Temperature = temperature.Value,
                FeelsLike = result.Daily.ApparentTemperature?[i]
                    ?? temperature.Value,
                Humidity = (int)Math.Round(
                    result.Daily.Humidity?[i] ?? 0),
                WindSpeed = result.Daily.WindSpeed?[i] ?? 0,
                WeatherCode = result.Daily.WeatherCode?[i] ?? 0,
                RecordedAt = DateTime.Parse(
                    result.Daily.Time[i],
                    CultureInfo.InvariantCulture)
            };

            await _repository.SaveAsync(item);
            statistics.Add(item);
        }

        return statistics;
    }


    private class GeocodingResponse
    {
        [JsonPropertyName("results")]
        public List<GeocodingCity>? Results { get; set; }
    }

    private class GeocodingCity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }

        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
    }

    private class ArchiveResponse
    {
        [JsonPropertyName("daily")]
        public DailyData? Daily { get; set; }
    }

    private class DailyData
    {
        [JsonPropertyName("time")]
        public List<string>? Time { get; set; }

        [JsonPropertyName("temperature_2m_mean")]
        public List<double?>? Temperature { get; set; }

        [JsonPropertyName("apparent_temperature_mean")]
        public List<double?>? ApparentTemperature { get; set; }

        [JsonPropertyName("relative_humidity_2m_mean")]
        public List<double?>? Humidity { get; set; }

        [JsonPropertyName("wind_speed_10m_mean")]
        public List<double?>? WindSpeed { get; set; }

        [JsonPropertyName("weather_code")]
        public List<int?>? WeatherCode { get; set; }
    }
}

public class CitySearchResult
{
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public override string ToString()
    {
        return $"{Name}, {Country}";
    }
}
