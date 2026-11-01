
using WeatherApp.Models;
using WeatherApp.Services;
using Microsoft.Maui.Graphics;
using Microsoft.Extensions.DependencyInjection;

namespace WeatherApp.Views;

public partial class StatisticsPage : ContentPage
{
    private readonly WeatherStatisticsService _statisticsService;

    private List<CitySearchResult> _cities = new();
    private CitySearchResult? _selectedCity;
    private List<CityWeatherStatistics> _statistics = new();

    private CancellationTokenSource? _searchCancellation;
    private bool _isSelectingCity = false;

    private async void OpenAdvancedStatistics_Clicked(object sender, EventArgs e)
    {
        if (_selectedCity == null)
        {
            await DisplayAlert(
                "Atentie",
                "Te rog sa selectezi mai intai un oraș.",
                "OK");
            return;
        }

        int days = PeriodPicker.SelectedIndex switch
        {
            0 => 7,
            1 => 30,
            2 => 90,
            3 => 180,
            4 => 270,
            5 => 365,
            _ => 7
        };

        await Navigation.PushModalAsync(
            new NavigationPage(
                new AdvancedStatisticsPage(
                    _selectedCity,
                    days,
                    _statisticsService)));
    }



    public StatisticsPage()
      : this(Application.Current!
          .Handler!
          .MauiContext!
          .Services
          .GetRequiredService<WeatherStatisticsService>())
    {
    }

    public StatisticsPage(WeatherStatisticsService statisticsService)
    {
        InitializeComponent();
        SetNeutralDashboard();

        _statisticsService = statisticsService;

        PeriodPicker.SelectedIndex = 0;

        UpdateCharts();
    }

    private async void CitySearchEntry_TextChanged(
        object sender, TextChangedEventArgs e)
    {
        if (_isSelectingCity)
            return;
        _searchCancellation?.Cancel();
        _searchCancellation?.Dispose();

        var cancellation = new CancellationTokenSource();
        _searchCancellation = cancellation;

        string searchText = e.NewTextValue?.Trim() ?? "";

        _selectedCity = null;
        SelectedCityLabel.Text = "Niciun oras selectat";

        if (searchText.Length < 2)
        {
            CitySuggestions.IsVisible = false;
            return;
        }

        try
        {
            await Task.Delay(400, cancellation.Token);

            var cities =
                await _statisticsService.SearchCitiesAsync(searchText);

            if (cancellation.IsCancellationRequested)
                return;

            _cities = cities;
            CitySuggestions.ItemsSource = _cities;
            CitySuggestions.IsVisible = _cities.Count > 0;
        }
        catch (OperationCanceledException)
        {
           
        }
        catch (Exception ex)
        {
            CitySuggestions.IsVisible = false;

            await DisplayAlert(
                "Eroare",
                $"Nu s-au putut cauta orasele: {ex.Message}",
                "OK");
        }
    }

    private void CitySuggestions_SelectionChanged(
        object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault()
            is not CitySearchResult city)
            return;

        _selectedCity = city;

        _isSelectingCity = true;
        CitySearchEntry.Text = city.Name;
        _isSelectingCity = false;
        SelectedCityLabel.Text = $"{city.Name}, {city.Country}";

        CitySuggestions.IsVisible = false;
        CitySuggestions.SelectedItem = null;
    }

    private void SetNeutralDashboard()
    {
        TemperatureCard.BackgroundColor = Color.FromArgb("#F1F3F5");
        HumidityCard.BackgroundColor = Color.FromArgb("#F1F3F5");
        GaugeCard.BackgroundColor = Color.FromArgb("#F1F3F5");
        WeatherDistributionCard.BackgroundColor = Color.FromArgb("#F1F3F5");
        BoxPlotCard.BackgroundColor = Color.FromArgb("#F1F3F5");
    }

    private void SetColoredDashboard()
    {
        TemperatureCard.BackgroundColor = Color.FromArgb("#FFF0E5");
        HumidityCard.BackgroundColor = Color.FromArgb("#E5F2FF");
        GaugeCard.BackgroundColor = Color.FromArgb("#E8F5F0");
        WeatherDistributionCard.BackgroundColor = Color.FromArgb("#F0EAFE");
        BoxPlotCard.BackgroundColor = Color.FromArgb("#FCE8EC");
    }

    private async void LoadStatistics_Clicked(
    object sender, EventArgs e)
    {
        if (_selectedCity == null)
        {
            await DisplayAlert(
                "Atentie",
                "Te rog sa cauti si sa selectezi un oras.",
                "OK");
            return;
        }

        int days = PeriodPicker.SelectedIndex switch
        {
            0 => 7,
            1 => 30,
            2 => 90,
            3 => 180,
            4 => 270,
            5 => 365,
            _ => 7
        };

        try
        {
            _statistics = await _statisticsService
                .GetHistoricalStatisticsAsync(_selectedCity, days);

            UpdateStatistics();
            DashboardSection.IsVisible = _statistics.Count > 0;

            if (_statistics.Count > 0)
                SetColoredDashboard();
            else
                SetNeutralDashboard();
        }
        catch (Exception ex)
        {
            DashboardSection.IsVisible = false;
            SetNeutralDashboard();

            await DisplayAlert(
                "Eroare",
                $"Nu s-au putut incarca statisticile: {ex.Message}",
                "OK");
        }
    }

    private void PeriodPicker_SelectedIndexChanged(
        object sender, EventArgs e)
    {
        // Datele se reîncarcă la apăsarea butonului.
    }

    private void UpdateStatistics()
    {
        if (_statistics.Count == 0)
        {
            StatisticsCollection.ItemsSource = null;

            UpdateCharts();
            return;
        }

        StatisticsCollection.ItemsSource =
            _statistics
                .OrderByDescending(x => x.RecordedAt)
                .ToList();

        UpdateCharts();
    }

    private void UpdateCharts()
    {
        var orderedStatistics = _statistics
            .OrderBy(x => x.RecordedAt)
            .ToList();

        int sunny = orderedStatistics.Count(x =>
    x.WeatherCode == 0 || x.WeatherCode == 1);

        int snowy = orderedStatistics.Count(x =>
            x.WeatherCode == 71 || x.WeatherCode == 73 ||
            x.WeatherCode == 75 || x.WeatherCode == 77 ||
            x.WeatherCode == 85 || x.WeatherCode == 86);

        int cloudy = orderedStatistics.Count(x =>
            x.WeatherCode == 2 || x.WeatherCode == 3 ||
            x.WeatherCode == 45 || x.WeatherCode == 48);

        int rainy = orderedStatistics.Count(x =>
            x.WeatherCode != 0 && x.WeatherCode != 1 &&
            x.WeatherCode != 2 && x.WeatherCode != 3 &&
            x.WeatherCode != 45 && x.WeatherCode != 48 &&
            x.WeatherCode != 71 && x.WeatherCode != 73 &&
            x.WeatherCode != 75 && x.WeatherCode != 77 &&
            x.WeatherCode != 85 && x.WeatherCode != 86);

        SunnyCountLabel.Text = $"Senin ({sunny})";
        CloudyCountLabel.Text = $"Innorat ({cloudy})";
        RainyCountLabel.Text = $"Ploaie ({rainy})";
        SnowyCountLabel.Text = $"Ninsoare ({snowy})";

        TemperatureChart.Drawable =
            new WeatherStatisticsChartDrawable(orderedStatistics);

        HumidityChart.Drawable =
            new HumidityAreaChartDrawable(orderedStatistics);

        TemperatureGauge.Drawable =
            new TemperatureGaugeDrawable(orderedStatistics);

        WeatherDistributionChart.Drawable =
            new WeatherDistributionDrawable(orderedStatistics);

        TemperatureBoxPlot.Drawable =
            new TemperatureBoxPlotDrawable(orderedStatistics);

        TemperatureChart.Invalidate();
        HumidityChart.Invalidate();
        TemperatureGauge.Invalidate();
        WeatherDistributionChart.Invalidate();
        TemperatureBoxPlot.Invalidate();
    }
}
public class HumidityAreaChartDrawable : IDrawable
{
    private readonly List<CityWeatherStatistics> _data;

    public HumidityAreaChartDrawable(
        List<CityWeatherStatistics> data)
    {
        _data = data;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (_data.Count == 0) return;

        float left = 40;
        float top = 20;
        float width = rect.Width - 55;
        float height = rect.Height - 55;

        if (width <= 0 || height <= 0) return;

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 11;

        for (int i = 0; i <= 4; i++)
        {
            float y = top + height * i / 4;

            canvas.StrokeColor = Color.FromArgb("#E0E0E0");
            canvas.StrokeSize = 1;
            canvas.DrawLine(left, y, left + width, y);

            canvas.DrawString(
                (100 - i * 25).ToString() + "%",
                0, y - 7, left - 5, 14,
                HorizontalAlignment.Right,
                VerticalAlignment.Center);
        }

        var points = new List<PointF>();

        for (int i = 0; i < _data.Count; i++)
        {
            float x = _data.Count == 1
                ? left + width / 2
                : left + width * i / (_data.Count - 1);

            float y = top + height -
                (float)Math.Clamp(_data[i].Humidity, 0, 100)
                / 100 * height;

            points.Add(new PointF(x, y));
        }

        var path = new PathF();
        path.MoveTo(points[0].X, top + height);
        path.LineTo(points[0].X, points[0].Y);

        foreach (var point in points.Skip(1))
            path.LineTo(point.X, point.Y);

        path.LineTo(points[^1].X, top + height);
        path.Close();

        canvas.FillColor = Color.FromArgb("#553F9BDE");
        canvas.FillPath(path);

        canvas.StrokeColor = Color.FromArgb("#2196F3");
        canvas.StrokeSize = 3;

        for (int i = 1; i < points.Count; i++)
        {
            canvas.DrawLine(
                points[i - 1].X,
                points[i - 1].Y,
                points[i].X,
                points[i].Y);
        }
    }
}
public class TemperatureGaugeDrawable : IDrawable
{
    private readonly List<CityWeatherStatistics> _data;

    public TemperatureGaugeDrawable(
        List<CityWeatherStatistics> data)
    {
        _data = data;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (_data.Count == 0) return;

        float centerX = rect.Width / 2;
        float centerY = rect.Height * 0.75f;
        float radius = Math.Min(rect.Width * 0.35f,
            rect.Height * 0.65f);

        float left = centerX - radius;
        float top = centerY - radius;

        canvas.StrokeSize = 18;
        canvas.StrokeLineCap = LineCap.Round;

        canvas.StrokeColor = Color.FromArgb("#E0E0E0");
        canvas.DrawArc(
            left, top, radius * 2, radius * 2,
            180, 360, true, false);

        double average = _data.Average(x => x.Temperature);

        float progress = (float)Math.Clamp(
            (average + 20) / 60, 0, 1);

        canvas.StrokeColor = Color.FromArgb("#2196F3");
        canvas.DrawArc(
            left, top, radius * 2, radius * 2,
            180, 180 + progress * 180, true, false);

        canvas.FontColor = Colors.Black;
        canvas.FontSize = 28;

        canvas.DrawString(
            $"{average:F1} °C",
            centerX - 65,
            centerY - 45,
            130, 40,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.FontSize = 12;
        canvas.FontColor = Colors.Gray;

        canvas.DrawString(
            "Temperatură medie",
            centerX - 75,
            centerY - 5,
            150, 25,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }
}
public class WeatherDistributionDrawable : IDrawable
{
    private readonly List<CityWeatherStatistics> _data;

    public WeatherDistributionDrawable(
        List<CityWeatherStatistics> data)
    {
        _data = data;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (_data.Count == 0) return;

        int sunny = 0;
        int cloudy = 0;
        int rainy = 0;
        int snowy = 0;

        foreach (var item in _data)
        {
            int code = item.WeatherCode;

            if (code == 0 || code == 1)
                sunny++;
            else if (code == 71 || code == 73 || code == 75 ||
                     code == 77 || code == 85 || code == 86)
                snowy++;
            else if (code == 2 || code == 3 || code == 45 || code == 48)
                cloudy++;
            else
                rainy++;
        }

        int total = sunny + cloudy + rainy + snowy;

        if (total == 0) return;

        float centerX = rect.Width / 2;
        float centerY = rect.Height / 2;
        float radius = Math.Min(rect.Width * 0.28f, rect.Height * 0.38f);

        float left = centerX - radius;
        float top = centerY - radius;
        float diameter = radius * 2;

        var categories = new[]
        {
            (Count: sunny, Color: Color.FromArgb("#FBC02D")),
            (Count: cloudy, Color: Color.FromArgb("#78909C")),
            (Count: rainy, Color: Color.FromArgb("#2196F3")),
            (Count: snowy, Color: Color.FromArgb("#80DEEA"))
        };

        canvas.StrokeSize = 22;
        canvas.StrokeLineCap = LineCap.Butt;

        float startAngle = -90;

        foreach (var category in categories)
        {
            if (category.Count == 0)
                continue;

            canvas.StrokeColor = category.Color;

            if (category.Count == total)
            {
                canvas.DrawCircle(centerX, centerY, radius);
                continue;
            }

            float sweepAngle = 360f * category.Count / total;

            canvas.DrawArc(
                left, top, diameter, diameter,
                startAngle,
                startAngle + sweepAngle,
                true,
                false);

            startAngle += sweepAngle;
        }

        canvas.FontColor = Colors.Black;
        canvas.FontSize = 21;

        canvas.DrawString(
            total.ToString(),
            centerX - 35,
            centerY - 15,
            70, 30,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;

        canvas.DrawString(
            "zile",
            centerX - 25,
            centerY + 12,
            50, 18,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);
    }
}
public class TemperatureBoxPlotDrawable : IDrawable
{
    private readonly List<CityWeatherStatistics> _data;

    public TemperatureBoxPlotDrawable(
        List<CityWeatherStatistics> data)
    {
        _data = data;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        if (_data.Count == 0) return;

        var values = _data
            .Select(x => x.Temperature)
            .OrderBy(x => x)
            .ToList();

        double min = values.First();
        double max = values.Last();
        double q1 = Percentile(values, 0.25);
        double median = Percentile(values, 0.50);
        double q3 = Percentile(values, 0.75);

        double range = Math.Max(max - min, 1);
        min -= range * 0.1;
        max += range * 0.1;

        float left = 30;
        float width = rect.Width - 60;
        float centerY = rect.Height / 2;

        if (width <= 0) return;

        float Map(double value) =>
            left + (float)((value - min) / (max - min)) * width;

        canvas.StrokeColor = Colors.Gray;
        canvas.StrokeSize = 2;

        canvas.DrawLine(
            Map(values.First()), centerY,
            Map(values.Last()), centerY);

        canvas.DrawLine(
            Map(values.First()), centerY - 12,
            Map(values.First()), centerY + 12);

        canvas.DrawLine(
            Map(values.Last()), centerY - 12,
            Map(values.Last()), centerY + 12);

        canvas.FillColor = Color.FromArgb("#90CAF9");

        canvas.FillRectangle(
            Map(q1), centerY - 25,
            Math.Max(Map(q3) - Map(q1), 1), 50);

        canvas.StrokeColor = Color.FromArgb("#1565C0");
        canvas.StrokeSize = 3;

        canvas.DrawLine(
            Map(median), centerY - 25,
            Map(median), centerY + 25);

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 11;

        canvas.DrawString(
            $"Min: {values.First():F1}°",
            left, centerY + 35, 75, 20,
            HorizontalAlignment.Left,
            VerticalAlignment.Center);

        canvas.DrawString(
            $"Mediană: {median:F1}°",
            rect.Width / 2 - 50, centerY + 35, 100, 20,
            HorizontalAlignment.Center,
            VerticalAlignment.Center);

        canvas.DrawString(
            $"Max: {values.Last():F1}°",
            rect.Width - 100, centerY + 35, 90, 20,
            HorizontalAlignment.Right,
            VerticalAlignment.Center);
    }

    private static double Percentile(
        List<double> values, double percentile)
    {
        double position = (values.Count - 1) * percentile;

        int lower = (int)Math.Floor(position);
        int upper = (int)Math.Ceiling(position);

        if (lower == upper)
            return values[lower];

        return values[lower] +
            (values[upper] - values[lower]) *
            (position - lower);
    }
}
