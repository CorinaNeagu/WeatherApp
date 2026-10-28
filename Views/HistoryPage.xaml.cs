
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.Views;

public partial class HistoryPage : ContentPage
{
    private readonly WeatherSearchService _weatherSearchService;

    private List<WeatherSearchHistory> _history = new();

    public HistoryPage(WeatherSearchService weatherSearchService)
    {
        InitializeComponent();

        _weatherSearchService = weatherSearchService;

        SortPicker.SelectedIndex = 0;
    }

    // Încărcăm istoricul când deschidem pagina.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadHistoryAsync();
    }

    private async Task LoadHistoryAsync()
    {
        try
        {
            _history = await _weatherSearchService.GetHistoryAsync();
            ApplyFilters();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Eroare",
                ex.Message,
                "OK");
        }
    }

    // Filtrare după oraș și sortare.
    private void ApplyFilters()
    {
        var result = _history.AsEnumerable();

        string search = HistorySearchBar.Text?.Trim() ?? "";

        if (!string.IsNullOrWhiteSpace(search))
        {
            result = result.Where(x =>
                x.CityName.Contains(
                    search,
                    StringComparison.OrdinalIgnoreCase));
        }

        result = SortPicker.SelectedIndex switch
        {
            1 => result.OrderBy(x => x.SearchedAt),
            2 => result.OrderBy(x => x.CityName),
            _ => result.OrderByDescending(x => x.SearchedAt)
        };

        var items = result.ToList();

        HistoryCollectionView.ItemsSource = items;

        CountLabel.Text = $"{items.Count} căutări";
    }

    private void OnFilterChanged(object sender, EventArgs e)
    {
        ApplyFilters();
    }

    // Ștergerea tuturor căutărilor.
    private async void OnClearHistoryClicked(
        object sender, EventArgs e)
    {
        if (_history.Count == 0)
        {
            await DisplayAlert(
                "Istoric",
                "Nu există căutări de șters.",
                "OK");

            return;
        }

        bool confirm = await DisplayAlert(
            "Confirmare",
            "Sigur dorești să ștergi tot istoricul?",
            "Da",
            "Nu");

        if (!confirm)
            return;

        try
        {
            await _weatherSearchService.ClearHistoryAsync();

            await LoadHistoryAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Eroare",
                ex.Message,
                "OK");
        }
    }
}
