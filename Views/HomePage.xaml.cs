namespace WeatherApp.Pages;

public class WeatherImage
{
    public string Image { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

public partial class HomePage : ContentPage
{
    private IDispatcherTimer? _timer;
    private CancellationTokenSource? _favoriteToastCancellation;
    private bool _isUserInteracting = false; 

    public List<WeatherImage> WeatherImages { get; set; } = new()
    {
        new WeatherImage
        {
            Image = "sunny.jpg",
            Title = "Vreme insorita",
            Description = "Bucura-te de o zi frumoasa!"
        },
        new WeatherImage
        {
            Image = "forest.jpg",
            Title = "Padure în natura",
            Description = "Descopera frumusetea naturii."
        },
        new WeatherImage
        {
            Image = "cloudy.jpg",
            Title = "Cer noros",
            Description = "O priveliste linistitoare."
        },
        new WeatherImage
        {
            Image = "sky.webp",
            Title = "Cer senin",
            Description = "Priveste cerul si relaxeaza-te."
        },
        new WeatherImage
        {
            Image = "storm.jpg",
            Title = "Furtuna",
            Description = "Urmareste avertizarile meteo."
        },
        new WeatherImage
        {
            Image = "snow.jpg",
            Title = "Vreme cu zapada",
            Description = "Bucura-te de peisajele de iarna!"
        },
        new WeatherImage
        {
            Image = "lightning.jpg",
            Title = "Furtuna cu fulgere",
            Description = "Urmareste avertizarile meteo!"
        },
    };

    public HomePage()
    {
        InitializeComponent();

        BindingContext = this;

        WeatherCarousel.IndicatorView = CarouselIndicators;
        WeatherCarousel.Position = 0;

        _timer = Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(3);
        _timer.Tick += (_, _) =>
        {
            if (_isUserInteracting || WeatherImages.Count <= 1)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                int currentPosition = WeatherCarousel.Position;
                int nextPosition = (currentPosition + 1) % WeatherImages.Count;

                WeatherCarousel.ScrollTo(nextPosition, animate: true);
            });
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (_timer != null && !_timer.IsRunning)
            _timer.Start();
    }

    protected override void OnDisappearing()
    {
        _timer?.Stop();
        base.OnDisappearing();
    }


    private async void FavoriteButton_Clicked(object sender, EventArgs e)
    {
        _favoriteToastCancellation?.Cancel();
        _favoriteToastCancellation?.Dispose();

        _favoriteToastCancellation = new CancellationTokenSource();
        var token = _favoriteToastCancellation.Token;

        FavoriteToast.IsVisible = true;
        FavoriteToast.Opacity = 0;

        await FavoriteToast.FadeTo(1, 250);

        try
        {
            await Task.Delay(2500, token);

            if (!token.IsCancellationRequested)
            {
                await FavoriteToast.FadeTo(0, 250);
                FavoriteToast.IsVisible = false;
            }
        }
        catch (TaskCanceledException)
        {
        }
    }

    private async void CloseFavoriteToast_Clicked(object sender, TappedEventArgs e)
    {
        _favoriteToastCancellation?.Cancel();

        await FavoriteToast.FadeTo(0, 200);
        FavoriteToast.IsVisible = false;
    }

    private async void CurrentWeatherCard_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("///currentweather");
    }

    private async void StatisticsCard_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("///dashboard");
    }

    private async void ForecastCard_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("///forecast");
    }

    private async void AirQualityCard_Tapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("///airquality");
    }

}