
namespace WeatherApp.Views;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
    }

    private async void BackButton_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void OpenMeteo_Clicked(object sender, EventArgs e)
    {
        await Launcher.Default.OpenAsync("https://open-meteo.com/");
    }
}
