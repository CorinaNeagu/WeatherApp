using Microsoft.Maui.Controls;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class ComparePage : ContentPage
{
    private readonly WeatherSearchService _weatherSearchService;

    public ComparePage(WeatherSearchService weatherSearchService)
    {
        InitializeComponent();
        _weatherSearchService = weatherSearchService;
    }

    private async void OnCompareClicked(object? sender, EventArgs e)
    {
        string city1 = City1Box.Text.Trim();
        string city2 = City2Box.Text.Trim();

        if (string.IsNullOrWhiteSpace(city1) || string.IsNullOrWhiteSpace(city2))
        {
            ShowError("Introdu ambele orașe.");
            return;
        }

        if (string.Equals(city1, city2, StringComparison.OrdinalIgnoreCase))
        {
            ShowError("Alege două orașe diferite.");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;
        SummaryCard.IsVisible = false;
        ResultsGrid.IsVisible = false;

        try
        {
            // pe rand, nu in paralel
            WeatherData w1 = await _weatherSearchService.SearchAndSaveAsync(city1);
            WeatherData w2 = await _weatherSearchService.SearchAndSaveAsync(city2);

            FillCard(w1, City1Label, Temp1Label, Condition1Label,
                     Feels1Label, Humidity1Label, Wind1Label);
            FillCard(w2, City2Label, Temp2Label, Condition2Label,
                     Feels2Label, Humidity2Label, Wind2Label);

            SummaryLabel.Text = BuildSummary(w1, w2);

            SummaryCard.IsVisible = true;
            ResultsGrid.IsVisible = true;
        }
        catch (HttpRequestException)
        {
            ShowError("Nu am putut accesa serviciul meteo. Verifică internetul.");
        }
        catch (InvalidOperationException ex)
        {
            ShowError(ex.Message);
        }
        catch (Exception)
        {
            ShowError("A apărut o eroare la preluarea sau salvarea datelor.");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }

    private static void FillCard(
        WeatherData w,
        Label city, Label temp, Label condition,
        Label feels, Label humidity, Label wind)
    {
        city.Text = $"{w.CityName}, {w.Country}";
        temp.Text = $"{w.Temperature:0.#} °C";
        condition.Text = GetCondition(w.WeatherCode);
        feels.Text = $"🌡️ Resimțită: {w.FeelsLike:0.#} °C";
        humidity.Text = $"💧 Umiditate: {w.Humidity}%";
        wind.Text = $"💨 Vânt: {w.WindSpeed:0.#} km/h";
    }

    private static string BuildSummary(WeatherData a, WeatherData b)
    {
        double diff = Math.Abs(a.Temperature - b.Temperature);

        if (diff < 0.5)
            return $"{a.CityName} și {b.CityName} au aproape aceeași temperatură.";

        string warmer = a.Temperature > b.Temperature ? a.CityName : b.CityName;
        return $"În {warmer} este mai cald cu {diff:0.#} °C.";
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }

    // la fel ca si in CurrentWeatherPage:
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