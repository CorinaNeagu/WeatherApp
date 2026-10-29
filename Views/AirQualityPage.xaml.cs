
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class AirQualityPage : ContentPage
{
    private readonly WeatherAPIService _weatherApi;

    public AirQualityPage(WeatherAPIService weatherApi)
    {
        InitializeComponent();
        _weatherApi = weatherApi;
    }

    private async void OnCheckClicked(object? sender, EventArgs e)
    {
        string city = CityEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(city))
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
            var data = await _weatherApi.GetAirQualityAsync(city);

            CityLabel.Text = data.City;
            AqiLabel.Text = data.EuropeanAqi.ToString("F0");

            Pm25Label.Text = $"{data.Pm25:F1} µg/m³";
            Pm10Label.Text = $"{data.Pm10:F1} µg/m³";
            No2Label.Text = $"{data.NitrogenDioxide:F1} µg/m³";
            OzoneLabel.Text = $"{data.Ozone:F1} µg/m³";

            double aqi = data.EuropeanAqi;

            if (aqi < 20)
            {
                AqiStatusLabel.Text = "Bună";
                AqiStatusLabel.TextColor = Color.FromArgb("#15803D");
            }
            else if (aqi < 40)
            {
                AqiStatusLabel.Text = "Acceptabilă";
                AqiStatusLabel.TextColor = Color.FromArgb("#65A30D");
            }
            else if (aqi < 60)
            {
                AqiStatusLabel.Text = "Moderată";
                AqiStatusLabel.TextColor = Color.FromArgb("#D97706");
            }
            else if (aqi < 80)
            {
                AqiStatusLabel.Text = "Slabă";
                AqiStatusLabel.TextColor = Color.FromArgb("#EA580C");
            }
            else if (aqi <= 100)
            {
                AqiStatusLabel.Text = "Foarte slabă";
                AqiStatusLabel.TextColor = Color.FromArgb("#DC2626");
            }
            else
            {
                AqiStatusLabel.Text = "Extrem de slabă";
                AqiStatusLabel.TextColor = Color.FromArgb("#991B1B");
            }
        }
        catch (HttpRequestException)
        {
            ErrorLabel.Text =
                "Nu pot accesa serviciul. Verifică conexiunea la internet.";
            ErrorLabel.IsVisible = true;
        }
        catch (Exception ex)
        {
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }
}
