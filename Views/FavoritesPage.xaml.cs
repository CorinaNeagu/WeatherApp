
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

    // Încărcăm favoritele când deschidem pagina.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadFavoritesAsync();
    }

    private async Task LoadFavoritesAsync()
    {
        try
        {
            var favorites =
                await _favoritesRepository.GetAllAsync();

            FavoritesCollectionView.ItemsSource = favorites;

            CountLabel.Text =
                $"{favorites.Count} orașe favorite";
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Eroare",
                ex.Message,
                "OK");
        }
    }

    // Adăugăm un oraș.
    private async void OnAddClicked(
        object sender, EventArgs e)
    {
        string cityName = CityBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(cityName))
        {
            await DisplayAlert(
                "Atenție",
                "Introdu numele unui oraș.",
                "OK");

            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        if (sender is Button button)
            button.IsEnabled = false;

        try
        {
            // Verificam orașul prin Open-Meteo.
            var selected = CityBox.SelectedCity;

            var weather = selected != null
                ? await _weatherApi.GetCurrentWeatherAsync(selected)
                : await _weatherApi.GetCurrentWeatherAsync(cityName);

            // Salvam orașul în SQLite.
            await _favoritesRepository.AddAsync(weather);

            CityBox.Text = "";

            await LoadFavoritesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Eroare",
                ex.Message,
                "OK");
        }

        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;

            if (sender is Button)
                ((Button)sender).IsEnabled = true;
        }



    }

    // Ștergem orașul selectat.
    private async void OnDeleteClicked(
        object sender, EventArgs e)
    {
        if (sender is not Button button ||
            button.CommandParameter is not int id)
            return;

        bool confirm = await DisplayAlert(
            "Confirmare",
            "Dorești să ștergi acest oraș din favorite?",
            "Da",
            "Nu");

        if (!confirm)
            return;

        try
        {
            await _favoritesRepository.DeleteAsync(id);

            await LoadFavoritesAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Eroare",
                ex.Message,
                "OK");
        }
    }


    private async void OnWeatherClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        string cityName = button.CommandParameter.ToString() ?? "";

        try
        {
            var weather = await _weatherApi.GetCurrentWeatherAsync(cityName);

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
