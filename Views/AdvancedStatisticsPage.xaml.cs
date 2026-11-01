
using Microsoft.Maui.Graphics;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class AdvancedStatisticsPage : ContentPage
{
    private readonly CitySearchResult _city;
    private readonly int _days;
    private readonly WeatherStatisticsService _statisticsService;

    private List<CityWeatherStatistics> _statistics = new();

    public AdvancedStatisticsPage(
        CitySearchResult city,
        int days,
        WeatherStatisticsService statisticsService)
    {
        InitializeComponent();

        _city = city;
        _days = days;
        _statisticsService = statisticsService;

        CityTitle.Text = $"{city.Name}, {city.Country}";
        PeriodTitle.Text = $"Analiza ultimelor {days} zile";

        Loaded += AdvancedStatisticsPage_Loaded;
    }

    private async void AdvancedStatisticsPage_Loaded(
        object? sender, EventArgs e)
    {
        Loaded -= AdvancedStatisticsPage_Loaded;

        await LoadAdvancedStatisticsAsync();
    }


    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }


    private async Task LoadAdvancedStatisticsAsync()
    {
        try
        {
            PeriodTitle.Text = "Se încarcă statisticile...";

            DateTime endDate = DateTime.Today.AddDays(-1);
            DateTime startDate = GetStartDate(endDate);

            _statistics = await _statisticsService
                .GetHistoricalStatisticsAsync(_city, _days, endDate);

            if (_statistics.Count == 0)
            {
                PeriodTitle.Text = "Nu există date disponibile.";
                return;
            }

            var previousEndDate = startDate.AddDays(-1);

            var previousStatistics = await _statisticsService
                .GetHistoricalStatisticsAsync(
                    _city,
                    _days,
                    previousEndDate);

            PeriodTitle.Text =
                $"Perioada: {startDate:dd.MM.yyyy} – {endDate:dd.MM.yyyy}";

            UpdateWindChart();
            UpdateComparison(previousStatistics);
            UpdateRecords();
            UpdateCalendar();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Eroare",
                $"Nu s-au putut încărca statisticile: {ex.Message}",
                "OK");
        }
    }

    private DateTime GetStartDate(DateTime endDate)
    {
        return _days switch
        {
            7 => endDate.AddDays(-6),
            30 => endDate.AddDays(-29),
            90 => endDate.AddMonths(-3).AddDays(1),
            180 => endDate.AddMonths(-6).AddDays(1),
            270 => endDate.AddMonths(-9).AddDays(1),
            365 => endDate.AddYears(-1).AddDays(1),
            _ => endDate.AddDays(-6)
        };
    }

    private void UpdateWindChart()
    {
        var ordered = _statistics
            .OrderBy(x => x.RecordedAt)
            .ToList();

        WindChart.Drawable =
            new WindChartDrawable(ordered);

        WindChart.Invalidate();
    }


    private void UpdateComparison(
        List<CityWeatherStatistics> previous)
    {
        if (previous.Count == 0)
        {
            ComparisonDescription.Text =
                "Nu există date suficiente pentru comparație.";

            TemperatureComparisonLabel.Text = "Indisponibil";
            HumidityComparisonLabel.Text = "Indisponibil";
            WindComparisonLabel.Text = "Indisponibil";

            TemperatureExplanation.Text =
                "Nu există valori pentru perioada precedentă.";

            HumidityExplanation.Text =
                "Nu există valori pentru perioada precedentă.";

            WindExplanation.Text =
                "Nu există valori pentru perioada precedentă.";

            return;
        }

        double currentTemp = _statistics.Average(x => x.Temperature);
        double previousTemp = previous.Average(x => x.Temperature);

        double currentHumidity = _statistics.Average(x => x.Humidity);
        double previousHumidity = previous.Average(x => x.Humidity);

        double currentWind = _statistics.Average(x => x.WindSpeed);
        double previousWind = previous.Average(x => x.WindSpeed);

        double tempDifference = currentTemp - previousTemp;
        double humidityDifference = currentHumidity - previousHumidity;
        double windDifference = currentWind - previousWind;

        TemperatureComparisonLabel.Text =
            $"{tempDifference:+0.0;-0.0;0.0} °C";

        HumidityComparisonLabel.Text =
            $"{humidityDifference:+0.0;-0.0;0.0} puncte procentuale";

        WindComparisonLabel.Text =
            $"{windDifference:+0.0;-0.0;0.0} km/h";

        TemperatureExplanation.Text =
            GetComparisonExplanation(
                "Temperatura medie",
                tempDifference,
                "°C");

        HumidityExplanation.Text =
            GetComparisonExplanation(
                "Umiditatea medie",
                humidityDifference,
                "puncte procentuale");

        WindExplanation.Text =
            GetComparisonExplanation(
                "Viteza medie a vântului",
                windDifference,
                "km/h");

        ComparisonDescription.Text =
            "Bara albastră reprezintă perioada precedentă, " +
            "iar bara colorată reprezintă perioada actuală.";

        ComparisonTemperatureChart.Drawable =
            new ComparisonBarChartDrawable(
                previousTemp,
                currentTemp,
                "#90A4AE",
                "#EF8354");

        ComparisonHumidityChart.Drawable =
            new ComparisonBarChartDrawable(
                previousHumidity,
                currentHumidity,
                "#90A4AE",
                "#26A6B8");

        ComparisonWindChart.Drawable =
            new ComparisonBarChartDrawable(
                previousWind,
                currentWind,
                "#90A4AE",
                "#26A69A");

        ComparisonTemperatureChart.Invalidate();
        ComparisonHumidityChart.Invalidate();
        ComparisonWindChart.Invalidate();
    }

    private string GetComparisonExplanation(
        string indicator,
        double difference,
        string unit)
    {
        if (Math.Abs(difference) < 0.05)
        {
            return $"{indicator} a rămas aproximativ la același nivel.";
        }

        string direction = difference > 0
            ? "a crescut"
            : "a scăzut";

        return $"{indicator} {direction} cu " +
               $"{Math.Abs(difference):F1} {unit} față de perioada precedentă.";
    }

    private void UpdateRecords()
    {
        var maxTemperature = _statistics
            .MaxBy(x => x.Temperature)!;

        var minTemperature = _statistics
            .MinBy(x => x.Temperature)!;

        var maxHumidity = _statistics
            .MaxBy(x => x.Humidity)!;

        var maxWind = _statistics
            .MaxBy(x => x.WindSpeed)!;

        MaxTemperatureLabel.Text =
            $"{maxTemperature.Temperature:F1} °C";

        MaxTemperatureDateLabel.Text =
            maxTemperature.RecordedAt.ToString("dd.MM.yyyy");

        MinTemperatureLabel.Text =
            $"{minTemperature.Temperature:F1} °C";

        MinTemperatureDateLabel.Text =
            minTemperature.RecordedAt.ToString("dd.MM.yyyy");

        MaxHumidityLabel.Text =
            $"{maxHumidity.Humidity}%";

        MaxHumidityDateLabel.Text =
            maxHumidity.RecordedAt.ToString("dd.MM.yyyy");

        MaxWindLabel.Text =
            $"{maxWind.WindSpeed:F1} km/h";

        MaxWindDateLabel.Text =
            maxWind.RecordedAt.ToString("dd.MM.yyyy");
    }

    private void UpdateCalendar()
    {
        var ordered = _statistics
            .OrderBy(x => x.RecordedAt)
            .ToList();

        if (ordered.Count == 0)
            return;

        int totalRows = 0;
        int totalMonths = 0;

        var months = ordered
            .GroupBy(x => new
            {
                x.RecordedAt.Year,
                x.RecordedAt.Month
            });

        foreach (var month in months)
        {
            DateTime firstDay = new DateTime(
                month.Key.Year,
                month.Key.Month,
                1);

            int offset =
                ((int)firstDay.DayOfWeek + 6) % 7;

            int daysInMonth = DateTime.DaysInMonth(
                month.Key.Year,
                month.Key.Month);

            totalRows += (offset + daysInMonth + 6) / 7;
            totalMonths++;
        }

        WeatherCalendar.HeightRequest =
            totalMonths * 65 + totalRows * 31 + 15;

        WeatherCalendar.Drawable =
            new WeatherCalendarDrawable(ordered);

        WeatherCalendar.Invalidate();
    }

    public class ComparisonBarChartDrawable : IDrawable
    {
        private readonly double _previous;
        private readonly double _current;
        private readonly Color _previousColor;
        private readonly Color _currentColor;

        public ComparisonBarChartDrawable(
            double previous,
            double current,
            string previousColor,
            string currentColor)
        {
            _previous = previous;
            _current = current;
            _previousColor = Color.FromArgb(previousColor);
            _currentColor = Color.FromArgb(currentColor);
        }

        public void Draw(ICanvas canvas, RectF rect)
        {
            float left = 12;
            float right = 12;

            if (rect.Width <= 150 || rect.Height <= 20)
                return;

            float barHeight = 22;
            float firstY = 12;
            float secondY = 57;

            float barStart = left + 60;
            float labelWidth = 48;
            float labelX = rect.Width - right - labelWidth;

            float maxBarWidth = labelX - barStart - 6;

            if (maxBarWidth <= 10)
                return;

            double maximum = Math.Max(
                Math.Max(Math.Abs(_previous), Math.Abs(_current)), 1);

            float previousWidth = (float)(
                Math.Abs(_previous) / maximum * maxBarWidth);

            float currentWidth = (float)(
                Math.Abs(_current) / maximum * maxBarWidth);

            canvas.FontSize = 10;
            canvas.FontColor = Colors.Gray;

            canvas.DrawString(
                "Anterior",
                left,
                firstY,
                55,
                barHeight,
                HorizontalAlignment.Left,
                VerticalAlignment.Center);

            canvas.DrawString(
                "Actual",
                left,
                secondY,
                55,
                barHeight,
                HorizontalAlignment.Left,
                VerticalAlignment.Center);

            canvas.FillColor = _previousColor;

            canvas.FillRoundedRectangle(
                barStart,
                firstY,
                Math.Max(previousWidth, 2),
                barHeight,
                6);

            canvas.FillColor = _currentColor;

            canvas.FillRoundedRectangle(
                barStart,
                secondY,
                Math.Max(currentWidth, 2),
                barHeight,
                6);

            canvas.FontColor = Colors.DarkSlateGray;
            canvas.FontSize = 10;

            canvas.DrawString(
                $"{_previous:F1}",
                labelX,
                firstY,
                labelWidth,
                barHeight,
                HorizontalAlignment.Right,
                VerticalAlignment.Center);

            canvas.DrawString(
                $"{_current:F1}",
                labelX,
                secondY,
                labelWidth,
                barHeight,
                HorizontalAlignment.Right,
                VerticalAlignment.Center);
        }
    }
    public class WindChartDrawable : IDrawable
    {
        private readonly List<CityWeatherStatistics> _data;

        public WindChartDrawable(
            List<CityWeatherStatistics> data)
        {
            _data = data;
        }

        public void Draw(ICanvas canvas, RectF rect)
        {
            if (_data.Count == 0)
                return;

            float left = 45;
            float right = 15;
            float top = 20;
            float bottom = 30;

            float width = rect.Width - left - right;
            float height = rect.Height - top - bottom;

            if (width <= 0 || height <= 0)
                return;

            double max = Math.Max(
                _data.Max(x => x.WindSpeed), 5);

            max = Math.Ceiling(max / 5) * 5;

            canvas.FontColor = Colors.Gray;
            canvas.FontSize = 10;
            canvas.StrokeSize = 1;

            for (int i = 0; i <= 4; i++)
            {
                float y = top + height * i / 4f;

                canvas.StrokeColor =
                    Color.FromArgb("#E0E0E0");

                canvas.DrawLine(
                    left, y,
                    left + width, y);

                double value = max - max * i / 4;

                canvas.DrawString(
                    $"{value:F0}",
                    0, y - 7, left - 5, 14,
                    HorizontalAlignment.Right,
                    VerticalAlignment.Center);
            }

            canvas.StrokeColor =
                Color.FromArgb("#00897B");

            canvas.StrokeSize = 3;

            float previousX = 0;
            float previousY = 0;

            for (int i = 0; i < _data.Count; i++)
            {
                float x = _data.Count == 1
                    ? left + width / 2
                    : left + width * i / (_data.Count - 1);

                float y = top + height -
                    (float)(_data[i].WindSpeed / max * height);

                if (i > 0)
                    canvas.DrawLine(previousX, previousY, x, y);

                canvas.FillColor =
                    Color.FromArgb("#00695C");

                canvas.FillCircle(x, y, 3);

                previousX = x;
                previousY = y;
            }

            canvas.FontSize = 9;
            canvas.FontColor = Colors.Gray;

            int labelCount = Math.Min(_data.Count, 5);

            for (int i = 0; i < labelCount; i++)
            {
                int index = labelCount == 1
                    ? 0
                    : (int)Math.Round(
                        i * (_data.Count - 1d) /
                        (labelCount - 1));

                float x = _data.Count == 1
                    ? left + width / 2
                    : left + width * index / (_data.Count - 1);

                canvas.DrawString(
                    _data[index].RecordedAt.ToString("dd.MM"),
                    x - 25,
                    top + height + 5,
                    50, 18,
                    HorizontalAlignment.Center,
                    VerticalAlignment.Center);
            }
        }
    }

    public class WeatherCalendarDrawable : IDrawable
    {
        private readonly List<CityWeatherStatistics> _data;

        private readonly string[] _months =
        {
        "Ianuarie", "Februarie", "Martie",
        "Aprilie", "Mai", "Iunie",
        "Iulie", "August", "Septembrie",
        "Octombrie", "Noiembrie", "Decembrie"
    };

        public WeatherCalendarDrawable(
            List<CityWeatherStatistics> data)
        {
            _data = data.OrderBy(x => x.RecordedAt).ToList();
        }

        private Color GetTemperatureColor(double temperature)
        {
            if (temperature < 0)
                return Color.FromArgb("#1565C0");

            if (temperature < 10)
                return Color.FromArgb("#42A5F5");

            if (temperature < 20)
                return Color.FromArgb("#FDD835");

            if (temperature < 30)
                return Color.FromArgb("#FB8C00");

            return Color.FromArgb("#E53935");
        }

        public void Draw(ICanvas canvas, RectF rect)
        {
            if (_data.Count == 0)
                return;

            string[] weekdays =
            {
            "L", "Ma", "Mi", "J", "V", "S", "D"
        };

            float left = 30;
            float cellWidth = (rect.Width - left - 10) / 7f;
            float cellHeight = 27;
            float gap = 4;
            float y = 0;

            var groupedMonths = _data
                .GroupBy(x => new
                {
                    x.RecordedAt.Year,
                    x.RecordedAt.Month
                })
                .OrderBy(x => x.Key.Year)
                .ThenBy(x => x.Key.Month);

            foreach (var month in groupedMonths)
            {
                DateTime firstDay = new DateTime(
                    month.Key.Year,
                    month.Key.Month,
                    1);

                int offset =
                    ((int)firstDay.DayOfWeek + 6) % 7;

                int daysInMonth = DateTime.DaysInMonth(
                    month.Key.Year,
                    month.Key.Month);

                canvas.FontColor = Color.FromArgb("#263445");
                canvas.FontSize = 15;
                canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;

                canvas.DrawString(
                    $"{_months[month.Key.Month - 1]} {month.Key.Year}",
                    left,
                    y,
                    rect.Width - left,
                    25,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Center);

                y += 28;

                canvas.FontColor = Colors.Gray;
                canvas.FontSize = 10;

                for (int i = 0; i < 7; i++)
                {
                    canvas.DrawString(
                        weekdays[i],
                        left + i * cellWidth,
                        y,
                        cellWidth,
                        18,
                        HorizontalAlignment.Center,
                        VerticalAlignment.Center);
                }

                y += 22;

                for (int day = 1; day <= daysInMonth; day++)
                {
                    int position = offset + day - 1;
                    int column = position % 7;
                    int row = position / 7;

                    var item = month.FirstOrDefault(x =>
                        x.RecordedAt.Day == day);

                    float x = left + column * cellWidth;
                    float cellY = y + row * (cellHeight + gap);

                    if (item != null)
                    {
                        canvas.FillColor =
                            GetTemperatureColor(item.Temperature);

                        canvas.FillRoundedRectangle(
                            x + 2,
                            cellY,
                            cellWidth - 4,
                            cellHeight,
                            5);

                        canvas.FontColor = Colors.Black;
                        canvas.FontSize = 9;

                        canvas.DrawString(
                            day.ToString(),
                            x,
                            cellY + 1,
                            cellWidth,
                            12,
                            HorizontalAlignment.Center,
                            VerticalAlignment.Center);

                        canvas.FontSize = 8;

                        canvas.DrawString(
                            $"{item.Temperature:F0}°",
                            x,
                            cellY + 13,
                            cellWidth,
                            12,
                            HorizontalAlignment.Center,
                            VerticalAlignment.Center);
                    }
                }

                int totalRows = (offset + daysInMonth + 6) / 7;

                y += totalRows * (cellHeight + gap) + 15;
            }
        }
    }
}
