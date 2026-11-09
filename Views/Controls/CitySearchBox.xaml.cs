using Microsoft.Extensions.DependencyInjection;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Controls;

public partial class CitySearchBox : ContentView
{
    private WeatherStatisticsService? _service;
    private CancellationTokenSource? _cts;
    private bool _isSelecting;

    public CitySearchResult? SelectedCity { get; private set; }

    public event EventHandler<CitySearchResult>? CitySelected;
    public event EventHandler? SearchCompleted;

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string),
            typeof(CitySearchBox), "Caută un oraș...",
            propertyChanged: (b, o, n) =>
                ((CitySearchBox)b).SearchEntry.Placeholder = (string)n);

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Text
    {
        get => SearchEntry.Text ?? "";
        set
        {
            _isSelecting = true;
            SearchEntry.Text = value;
            _isSelecting = false;
            SelectedCity = null;
        }
    }

    public CitySearchBox()
    {
        InitializeComponent();
        SearchEntry.Placeholder = Placeholder;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        _service ??= Handler?.MauiContext?.Services
            .GetService<WeatherStatisticsService>();
    }

    private async void SearchEntry_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (_isSelecting) return;

        _cts?.Cancel();
        _cts?.Dispose();
        var cts = new CancellationTokenSource();
        _cts = cts;

        SelectedCity = null;
        string text = e.NewTextValue?.Trim() ?? "";

        if (text.Length < 2 || _service == null)
        {
            Suggestions.IsVisible = false;
            return;
        }

        try
        {
            await Task.Delay(400, cts.Token);
            var cities = await _service.SearchCitiesAsync(text);

            if (cts.IsCancellationRequested) return;

            Suggestions.ItemsSource = cities;
            Suggestions.IsVisible = cities.Count > 0;
        }
        catch (OperationCanceledException) { }
        catch
        {
            Suggestions.IsVisible = false;
        }
    }

    private void Suggestions_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not CitySearchResult city)
            return;

        Text = city.Name;
        SelectedCity = city;

        Suggestions.IsVisible = false;
        Suggestions.SelectedItem = null;

        CitySelected?.Invoke(this, city);
    }

    private void SearchEntry_Completed(object? sender, EventArgs e)
    {
        Suggestions.IsVisible = false;
        SearchCompleted?.Invoke(this, EventArgs.Empty);
    }
}