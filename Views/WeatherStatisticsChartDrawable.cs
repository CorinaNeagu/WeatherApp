
using Microsoft.Maui.Graphics;
using WeatherApp.Models;

namespace WeatherApp.Views;

public class WeatherStatisticsChartDrawable : IDrawable
{
    private readonly List<CityWeatherStatistics> _statistics;

    public WeatherStatisticsChartDrawable(
        List<CityWeatherStatistics> statistics)
    {
        _statistics = statistics ?? new List<CityWeatherStatistics>();
    }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        float width = dirtyRect.Width;
        float height = dirtyRect.Height;

        if (_statistics.Count == 0 || width <= 0 || height <= 0)
            return;

        const float left = 48;
        const float right = 15;
        const float top = 20;
        const float bottom = 35;

        float chartWidth = width - left - right;
        float chartHeight = height - top - bottom;

        if (chartWidth <= 0 || chartHeight <= 0)
            return;

        double minTemperature =
            _statistics.Min(x => x.Temperature);

        double maxTemperature =
            _statistics.Max(x => x.Temperature);

        if (Math.Abs(maxTemperature - minTemperature) < 0.1)
        {
            minTemperature -= 1;
            maxTemperature += 1;
        }

        double range = maxTemperature - minTemperature;

        minTemperature -= range * 0.1;
        maxTemperature += range * 0.1;

        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 10;
        canvas.StrokeSize = 1;

        // Linii orizontale și valori pe axa verticală.
        for (int i = 0; i <= 4; i++)
        {
            float y = top + chartHeight * i / 4f;

            canvas.StrokeColor = Color.FromArgb("#E0E0E0");
            canvas.DrawLine(left, y, width - right, y);

            double value =
                maxTemperature -
                (maxTemperature - minTemperature) * i / 4;

            canvas.DrawString(
                $"{value:F1}°",
                0,
                y - 7,
                left - 6,
                14,
                HorizontalAlignment.Right,
                VerticalAlignment.Center);
        }

        // Axe.
        canvas.StrokeColor = Colors.Gray;

        canvas.DrawLine(
            left, top,
            left, top + chartHeight);

        canvas.DrawLine(
            left, top + chartHeight,
            width - right, top + chartHeight);

        // Linia temperaturii și punctele măsurătorilor.
        canvas.StrokeColor = Color.FromArgb("#2196F3");
        canvas.StrokeSize = 3;

        float previousX = 0;
        float previousY = 0;

        for (int i = 0; i < _statistics.Count; i++)
        {
            float x = _statistics.Count == 1
                ? left + chartWidth / 2
                : left + chartWidth * i / (_statistics.Count - 1);

            float y = top + chartHeight -
                (float)(
                    (_statistics[i].Temperature - minTemperature) /
                    (maxTemperature - minTemperature) *
                    chartHeight);

            if (i > 0)
                canvas.DrawLine(previousX, previousY, x, y);

            canvas.FillColor = Color.FromArgb("#1565C0");
            canvas.FillCircle(x, y, 4);

            previousX = x;
            previousY = y;
        }

        // Date pe axa orizontală.
        canvas.FontColor = Colors.Gray;
        canvas.FontSize = 9;

        int labelCount = Math.Min(_statistics.Count, 5);

        for (int i = 0; i < labelCount; i++)
        {
            int index = labelCount == 1
                ? 0
                : (int)Math.Round(
                    i * (_statistics.Count - 1d) /
                    (labelCount - 1));

            float x = _statistics.Count == 1
                ? left + chartWidth / 2
                : left + chartWidth * index /
                    (_statistics.Count - 1);

            string date =
                _statistics[index].RecordedAt.ToString("dd.MM");

            canvas.DrawString(
                date,
                x - 25,
                top + chartHeight + 7,
                50,
                18,
                HorizontalAlignment.Center,
                VerticalAlignment.Center);
        }
    }
}
