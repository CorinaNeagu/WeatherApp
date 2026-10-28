using System;
using System.Collections.Generic;
using System.Text;
using WeatherApp.Models;
using WeatherApp.Repositories;

/*
namespace WeatherApp.Services
{
    internal class WeatherSearchService
    {
    }
}
*/

namespace WeatherApp.Services;

public class WeatherSearchService
{
    private readonly WeatherAPIService _weatherApi;
    private readonly WeatherHistoryRepository _historyRepository;

    public WeatherSearchService(
        WeatherAPIService weatherApi,
        WeatherHistoryRepository historyRepository)
    {
        _weatherApi = weatherApi;
        _historyRepository = historyRepository;
    }

    public async Task<WeatherData> SearchAndSaveAsync(string cityName)
    {
        // Ne asigurăm că tabelul există.
        await _historyRepository.InitializeAsync();

        // Preluăm datele reale de la Open-Meteo.
        WeatherData weather =
            await _weatherApi.GetCurrentWeatherAsync(cityName);

        // Salvăm căutarea în SQLite.
        await _historyRepository.SaveAsync(weather);

        return weather;
    }

    public async Task<List<WeatherSearchHistory>> GetHistoryAsync()
    {
        await _historyRepository.InitializeAsync();

        return await _historyRepository.GetAllAsync();
    }


    public async Task ClearHistoryAsync()
    {
        await _historyRepository.DeleteAllAsync();
    }

}
