
using WeatherApp.Models;
using WeatherApp.Repositories;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class FavoritesPage : ContentPage
{
    private readonly FavoritesRepository _favoritesRepository;
    private readonly WeatherAPIService _weatherApi;

    public FavoritesPage(
        FavoritesRepository favoritesRepository,
        WeatherAPIService weatherApi)
    {
        InitializeComponent();
        _favoritesRepository = favoritesRepository;
        _weatherApi = weatherApi;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var favorites = await _favoritesRepository.GetAllAsync();

            FavoritesCollectionView.ItemsSource = favorites;
            CountLabel.Text = $"{favorites.Count} orașe favorite";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
    private async void OnAddClicked(object sender, EventArgs e)
    {
        string cityName = "";

        if (CityEntry.Text != null)
            cityName = CityEntry.Text.Trim();

        if (cityName == "")
        {
            await DisplayAlert("Atenție", "Introdu un oraș.", "OK");
            return;
        }

        try
        {
            var weather = await _weatherApi.GetCurrentWeatherAsync(cityName);
            bool added = await _favoritesRepository.AddAsync(weather);

            if (added)
            {
                CityEntry.Text = "";
                await LoadFavoritesAsync();
            }
            else
            {
                await DisplayAlert("Atenție", "Orașul există deja în favorite.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
    private async void OnUpdateWeatherClicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        if (button == null)
            return;

        FavoriteCity city = button.BindingContext as FavoriteCity;
        if (city == null)
            return;

        try
        {
            var weather = await _weatherApi.GetCurrentWeatherAsync(city.CityName);

            await _favoritesRepository.UpdateWeatherAsync(
                city.Id, weather.Temperature);

            await LoadFavoritesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
    private async void OnDeleteClicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        if (button == null)
            return;

        FavoriteCity city = button.BindingContext as FavoriteCity;
        if (city == null)
            return;

        bool confirm = await DisplayAlert(
            "Confirmare",
            "Dorești să ștergi acest oraș?",
            "Da", "Nu");

        if (confirm)
        {
            try
            {
                await _favoritesRepository.DeleteAsync(city.Id);
                await LoadFavoritesAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Eroare", ex.Message, "OK");
            }
        }
    }

    private async void OnWeatherClicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        if (button == null)
            return;

        FavoriteCity city = button.BindingContext as FavoriteCity;
        if (city == null)
            return;

        try
        {
            var weather = await _weatherApi.GetCurrentWeatherAsync(city.CityName);

            await DisplayAlert(
                $"Vremea în {weather.CityName}",
                $"Temperatura: {weather.Temperature:0.#} °C\n" +
                $"Umiditate: {weather.Humidity}%\n" +
                $"Vânt: {weather.WindSpeed:0.#} km/h",
                "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Eroare", ex.Message, "OK");
        }
    }
}
