using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Models;

namespace WeatherApp.Repositories;

public class WeatherStatisticsRepository
{
    private readonly string _databasePath;

    public WeatherStatisticsRepository()
    {
        _databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "weather.db");
    }

    private SqliteConnection CreateConnection()
    {
        return new SqliteConnection(
            $"Data Source={_databasePath}");
    }

    public async Task InitializeAsync()
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            CREATE TABLE IF NOT EXISTS CityWeatherStatistics (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CityName TEXT NOT NULL,
                Country TEXT,
                Latitude REAL NOT NULL,
                Longitude REAL NOT NULL,
                Temperature REAL NOT NULL,
                FeelsLike REAL NOT NULL,
                Humidity INTEGER NOT NULL,
                WindSpeed REAL NOT NULL,
                WeatherCode INTEGER NOT NULL,
                RecordedAt TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveAsync(CityWeatherStatistics data)
    {
        await InitializeAsync();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO CityWeatherStatistics
            (
                CityName, Country, Latitude, Longitude,
                Temperature, FeelsLike, Humidity,
                WindSpeed, WeatherCode, RecordedAt
            )
            VALUES
            (
                $city, $country, $latitude, $longitude,
                $temperature, $feelsLike, $humidity,
                $windSpeed, $weatherCode, $recordedAt
            );
            """;

        command.Parameters.AddWithValue("$city", data.CityName);
        command.Parameters.AddWithValue("$country", data.Country);
        command.Parameters.AddWithValue("$latitude", data.Latitude);
        command.Parameters.AddWithValue("$longitude", data.Longitude);
        command.Parameters.AddWithValue("$temperature", data.Temperature);
        command.Parameters.AddWithValue("$feelsLike", data.FeelsLike);
        command.Parameters.AddWithValue("$humidity", data.Humidity);
        command.Parameters.AddWithValue("$windSpeed", data.WindSpeed);
        command.Parameters.AddWithValue("$weatherCode", data.WeatherCode);
        command.Parameters.AddWithValue(
            "$recordedAt",
            data.RecordedAt.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<CityWeatherStatistics>> GetAllAsync()
    {
        await InitializeAsync();

        var statistics = new List<CityWeatherStatistics>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, CityName, Country, Latitude, Longitude,
                   Temperature, FeelsLike, Humidity, WindSpeed,
                   WeatherCode, RecordedAt
            FROM CityWeatherStatistics
            ORDER BY RecordedAt DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            statistics.Add(new CityWeatherStatistics
            {
                Id = reader.GetInt32(0),
                CityName = reader.GetString(1),
                Country = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Latitude = reader.GetDouble(3),
                Longitude = reader.GetDouble(4),
                Temperature = reader.GetDouble(5),
                FeelsLike = reader.GetDouble(6),
                Humidity = reader.GetInt32(7),
                WindSpeed = reader.GetDouble(8),
                WeatherCode = reader.GetInt32(9),
                RecordedAt = DateTime.Parse(reader.GetString(10))
            });
        }

        return statistics;
    }

    public async Task<List<CityWeatherStatistics>> GetByCityAsync(
        string cityName)
    {
        var all = await GetAllAsync();

        return all
            .Where(x => x.CityName == cityName)
            .ToList();
    }
}