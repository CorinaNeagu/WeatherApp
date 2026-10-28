using System;
using System.Collections.Generic;
using System.Text;

/*
namespace WeatherApp.Repositories
{
    internal class WeatherHistoryRepository
    {
    }
}*/

using Microsoft.Data.Sqlite;
using WeatherApp.Models;

namespace WeatherApp.Repositories;

public class WeatherHistoryRepository
{
    private readonly string _databasePath;

    public WeatherHistoryRepository()
    {
        _databasePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "weather.db");

        System.Diagnostics.Debug.WriteLine(
          $"CALE BAZA DE DATE: {_databasePath}");
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
            CREATE TABLE IF NOT EXISTS WeatherHistory (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CityName TEXT NOT NULL,
                Country TEXT,
                Temperature REAL NOT NULL,
                FeelsLike REAL NOT NULL,
                Humidity INTEGER NOT NULL,
                WindSpeed REAL NOT NULL,
                WeatherCode INTEGER NOT NULL,
                SearchedAt TEXT NOT NULL
            );
            """;

        await command.ExecuteNonQueryAsync();
    }

    public async Task SaveAsync(WeatherData weather)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO WeatherHistory
            (CityName, Country, Temperature, FeelsLike,
             Humidity, WindSpeed, WeatherCode, SearchedAt)
            VALUES
            ($city, $country, $temperature, $feelsLike,
             $humidity, $windSpeed, $weatherCode, $searchedAt);
            """;

        command.Parameters.AddWithValue("$city", weather.CityName);
        command.Parameters.AddWithValue("$country", weather.Country);
        command.Parameters.AddWithValue("$temperature", weather.Temperature);
        command.Parameters.AddWithValue("$feelsLike", weather.FeelsLike);
        command.Parameters.AddWithValue("$humidity", weather.Humidity);
        command.Parameters.AddWithValue("$windSpeed", weather.WindSpeed);
        command.Parameters.AddWithValue("$weatherCode", weather.WeatherCode);
        command.Parameters.AddWithValue(
            "$searchedAt",
            DateTime.Now.ToString("O"));

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<WeatherSearchHistory>> GetAllAsync()
    {
        var history = new List<WeatherSearchHistory>();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        var command = connection.CreateCommand();

        command.CommandText = """
            SELECT Id, CityName, Country, Temperature, FeelsLike,
                   Humidity, WindSpeed, WeatherCode, SearchedAt
            FROM WeatherHistory
            ORDER BY Id DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            history.Add(new WeatherSearchHistory
            {
                Id = reader.GetInt32(0),
                CityName = reader.GetString(1),
                Country = reader.IsDBNull(2) ? "" : reader.GetString(2),
                Temperature = reader.GetDouble(3),
                FeelsLike = reader.GetDouble(4),
                Humidity = reader.GetInt32(5),
                WindSpeed = reader.GetDouble(6),
                WeatherCode = reader.GetInt32(7),
                SearchedAt = DateTime.Parse(reader.GetString(8))
            });
        }

        return history;
    }


    public async Task DeleteAllAsync()
    {
        await InitializeAsync();

        await using var connection = CreateConnection();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM WeatherHistory";

        await command.ExecuteNonQueryAsync();
    }

}

