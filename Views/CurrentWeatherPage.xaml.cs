
using Microsoft.Maui.Controls;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class CurrentWeatherPage : ContentPage
{
    private readonly WeatherSearchService _weatherSearchService;

    public CurrentWeatherPage(
        WeatherSearchService weatherSearchService)
    {
        InitializeComponent();
        _weatherSearchService = weatherSearchService;
    }

    private async void OnSearchClicked(object? sender, EventArgs e)
    {
        string cityName = CityEntry.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(cityName))
        {
            ErrorLabel.Text = "Introdu numele unui oraș.";
            ErrorLabel.IsVisible = true;
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;

        try
        {
            WeatherData weather =
                await _weatherSearchService.SearchAndSaveAsync(cityName);

            CityLabel.Text = $"{weather.CityName}, {weather.Country}";
            TemperatureLabel.Text = $"{weather.Temperature:0.#} °C";
            ConditionLabel.Text = GetCondition(weather.WeatherCode);
            FeelsLikeLabel.Text = $"{weather.FeelsLike:0.#} °C";
            HumidityLabel.Text = $"{weather.Humidity}%";
            WindLabel.Text = $"{weather.WindSpeed:0.#} km/h";
            UpdatedLabel.Text = weather.RetrievedAt.ToString("HH:mm");

            WeatherCard.IsVisible = true;
        }
        catch (HttpRequestException)
        {
            ErrorLabel.Text =
                "Nu am putut accesa serviciul meteo. Verifică internetul.";
            ErrorLabel.IsVisible = true;
        }
        catch (InvalidOperationException ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        catch (Exception)
        {
            ErrorLabel.Text =
                "A apărut o eroare la preluarea sau salvarea datelor.";
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private static string GetCondition(int code)
    {
        return code switch
        {
            0 => "Senin",
            1 => "Predominant senin",
            2 => "Parțial noros",
            3 => "Noros",
            45 or 48 => "Ceață",
            51 or 53 or 55 => "Burniță",
            61 or 63 or 65 => "Ploaie",
            71 or 73 or 75 => "Ninsoare",
            80 or 81 or 82 => "Averse de ploaie",
            85 or 86 => "Averse de ninsoare",
            95 or 96 or 99 => "Furtună",
            _ => "Condiții meteo variabile"
        };
    }
}
