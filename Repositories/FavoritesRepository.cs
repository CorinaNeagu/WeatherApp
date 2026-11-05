using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;
using WeatherApp.Models;

namespace WeatherApp.Repositories;

public class FavoritesRepository
{
    string path = Path.Combine(
        FileSystem.AppDataDirectory, "weather.db");

    public async Task InitAsync()
    {
        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();
        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS FavoriteCities (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                CityName TEXT NOT NULL,
                Country TEXT NOT NULL
            );
            """;

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<FavoriteCity>> GetAllAsync()
    {
        await InitAsync();

        var lista = new List<FavoriteCity>();

        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();
        cmd.CommandText =
            "SELECT Id, CityName, Country FROM FavoriteCities ORDER BY CityName";

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(new FavoriteCity
            {
                Id = reader.GetInt32(0),
                CityName = reader.GetString(1),
                Country = reader.GetString(2)
            });
        }

        return lista;
    }

    public async Task<bool> AddAsync(WeatherData weather)
    {
        await InitAsync();

        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();

        cmd.CommandText = """
            INSERT INTO FavoriteCities (CityName, Country)
            SELECT $city, $country
            WHERE NOT EXISTS (
                SELECT 1 FROM FavoriteCities
                WHERE CityName = $city COLLATE NOCASE
                AND Country = $country COLLATE NOCASE
            );
            """;

        cmd.Parameters.AddWithValue("$city", weather.CityName);
        cmd.Parameters.AddWithValue("$country", weather.Country);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task DeleteAsync(int id)
    {
        await InitAsync();

        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();
        cmd.CommandText =
            "DELETE FROM FavoriteCities WHERE Id = $id";

        cmd.Parameters.AddWithValue("$id", id);

        await cmd.ExecuteNonQueryAsync();
    }
}

