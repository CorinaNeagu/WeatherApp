
using Microsoft.Data.Sqlite;
using WeatherApp.Models;

namespace WeatherApp.Repositories;

public class FavoritesRepository
{
    private readonly string path = Path.Combine(
        FileSystem.AppDataDirectory, "weather.db");

    public async Task InitAsync()
    {
        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();

        cmd.CommandText = """
            CREATE TABLE IF NOT EXISTS FavoriteCities (Id INTEGER PRIMARY KEY AUTOINCREMENT, CityName TEXT NOT NULL,
                                                        Country TEXT NOT NULL, LastTemperature REAL, PreviousTemperature REAL,
                                                        LastCheckedAt TEXT, PreviousCheckedAt TEXT)
            """;

        await cmd.ExecuteNonQueryAsync();

        var columns = new List<string>();

        cmd.CommandText = "PRAGMA table_info(FavoriteCities)";

        using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                columns.Add(reader.GetString(1));
            }
        }

        string[] newColumns =
        {
            "LastTemperature REAL",
            "PreviousTemperature REAL",
            "LastCheckedAt TEXT",
            "PreviousCheckedAt TEXT"
        };

        foreach (string column in newColumns)
        {
            string name = column.Split(' ')[0];

            if (!columns.Contains(name))
            {
                cmd.CommandText ="ALTER TABLE FavoriteCities ADD COLUMN " + column;

                await cmd.ExecuteNonQueryAsync();
            }
        }
    }

    public async Task<List<FavoriteCity>> GetAllAsync()
    {
        await InitAsync();

        var favorites = new List<FavoriteCity>();

        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();

        cmd.CommandText = """
            SELECT Id, CityName, Country, LastTemperature, PreviousTemperature, LastCheckedAt, PreviousCheckedAt
            FROM FavoriteCities
            ORDER BY CityName
            """;

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var city = new FavoriteCity
            {
                Id = reader.GetInt32(0),
                CityName = reader.GetString(1),
                Country = reader.GetString(2)
            };

            if (!reader.IsDBNull(3))
                city.LastTemperature = reader.GetDouble(3);

            if (!reader.IsDBNull(4))
                city.PreviousTemperature = reader.GetDouble(4);

            if (!reader.IsDBNull(5))
                city.LastCheckedAt = reader.GetString(5);

            if (!reader.IsDBNull(6))
                city.PreviousCheckedAt = reader.GetString(6);

            favorites.Add(city);
        }

        return favorites;
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
            )
            """;

        cmd.Parameters.AddWithValue("$city", weather.CityName);
        cmd.Parameters.AddWithValue("$country", weather.Country);

        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task UpdateWeatherAsync(int id, double temperature)
    {
        await InitAsync();

        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();

        cmd.CommandText = """
            UPDATE FavoriteCities
            SET PreviousTemperature = LastTemperature,
                PreviousCheckedAt = LastCheckedAt,
                LastTemperature = $temperature,
                LastCheckedAt = $date
            WHERE Id = $id
            """;

        cmd.Parameters.AddWithValue("$temperature", temperature);
        cmd.Parameters.AddWithValue(
            "$date", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
        cmd.Parameters.AddWithValue("$id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await InitAsync();

        using var db = new SqliteConnection($"Data Source={path}");
        await db.OpenAsync();

        using var cmd = db.CreateCommand();

        cmd.CommandText = "DELETE FROM FavoriteCities WHERE Id = $id";
        cmd.Parameters.AddWithValue("$id", id);

        await cmd.ExecuteNonQueryAsync();
    }
}
