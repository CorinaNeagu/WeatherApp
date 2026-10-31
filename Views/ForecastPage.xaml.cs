
using Microsoft.Maui.Graphics;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class ForecastPage : ContentPage
{
    private readonly WeatherAPIService _weatherApi;

    public ForecastPage(WeatherAPIService weatherApi)
    {
        InitializeComponent();
        _weatherApi = weatherApi;

        ForecastChart.Drawable = new ForecastChartDrawable(
            new List<DailyForecast>());
    }

    private async void OnForecastClicked(
        object? sender, EventArgs e)
    {
        string city = CityEntry.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(city))
        {
            ErrorLabel.Text = "Introdu numele unui oraș.";
            ErrorLabel.IsVisible = true;
            return;
        }

        int[] periods = { 3, 5, 7, 14 };
        int selectedDays = periods[
            Math.Max(0, DaysPicker.SelectedIndex)];

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;
        ErrorLabel.IsVisible = false;

        try
        {
            List<DailyForecast> forecast =
                await _weatherApi.GetForecastAsync(city, selectedDays);

            ForecastList.ItemsSource = forecast;

            ForecastChart.Drawable =
                new ForecastChartDrawable(forecast);

            ForecastChart.Invalidate();
        }
        catch (HttpRequestException)
        {
            ErrorLabel.Text =
                "Nu pot accesa serviciul meteo. Verifică internetul.";
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

public class ForecastChartDrawable : IDrawable
{
    private readonly List<DailyForecast> _forecast;

    public ForecastChartDrawable(List<DailyForecast> forecast)
    {
        _forecast = forecast;
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        if (_forecast.Count == 0)
        {
            canvas.FontColor = Colors.Gray;
            canvas.FontSize = 14;
            canvas.DrawString(
                "Caută un oraș pentru a vedea graficul.",
                5, 5, dirtyRect.Width - 10, dirtyRect.Height - 10,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
            return;
        }

        float left = 38;
        float right = dirtyRect.Width - 15;
        float top = 25;
        float bottom = dirtyRect.Height - 35;

        double minTemp = _forecast.Min(x => x.MinTemperature);
        double maxTemp = _forecast.Max(x => x.MaxTemperature);

        if (maxTemp - minTemp < 1)
        {
            minTemp -= 1;
            maxTemp += 1;
        }

        float ChartY(double temp) =>
            bottom - (float)((temp - minTemp) /
            (maxTemp - minTemp)) * (bottom - top);

        float ChartX(int i) =>
            _forecast.Count == 1
                ? (left + right) / 2
                : left + i * (right - left) /
                  (_forecast.Count - 1);

        canvas.FontSize = 10;
        canvas.FontColor = Colors.Gray;
        canvas.StrokeColor = Color.FromArgb("#E2E8F0");
        canvas.StrokeSize = 1;

        for (int i = 0; i < 4; i++)
        {
            float y = top + i * (bottom - top) / 3;
            canvas.DrawLine(left, y, right, y);

            double value = maxTemp -
                i * (maxTemp - minTemp) / 3;

            canvas.DrawString(
                $"{value:F0}°",
                0, y - 8, 34, 18,
                HorizontalAlignment.Right,
                VerticalAlignment.Center);
        }

        DrawLine(
            canvas,
            _forecast.Select(x => x.MaxTemperature).ToList(),
            ChartX, ChartY,
            Color.FromArgb("#E58B35"));

        DrawLine(
            canvas,
            _forecast.Select(x => x.MinTemperature).ToList(),
            ChartX, ChartY,
            Color.FromArgb("#2563EB"));

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 9;

        for (int i = 0; i < _forecast.Count; i++)
        {
            string label = DateTime.TryParse(
                _forecast[i].Date, out var date)
                ? date.ToString("dd.MM")
                : _forecast[i].Date;

            canvas.DrawString(
                label,
                ChartX(i) - 20, bottom + 8, 40, 18,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }

    private static void DrawLine(
        ICanvas canvas,
        List<double> values,
        Func<int, float> chartX,
        Func<double, float> chartY,
        Color color)
    {
        if (values.Count == 0)
            return;

        canvas.StrokeColor = color;
        canvas.StrokeSize = 2.5f;

        for (int i = 0; i < values.Count - 1; i++)
        {
            canvas.DrawLine(
                chartX(i), chartY(values[i]),
                chartX(i + 1), chartY(values[i + 1]));
        }

        canvas.FillColor = color;

        for (int i = 0; i < values.Count; i++)
        {
            canvas.FillCircle(
                chartX(i), chartY(values[i]), 3.5f);
        }
    }
}
