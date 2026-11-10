
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Syncfusion.Maui.Toolkit.Hosting;
using System.Net.Http;
using WeatherApp.Services;
using WeatherApp.Repositories;

namespace WeatherApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            SQLitePCL.Batteries_V2.Init();

            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureSyncfusionToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if WINDOWS
                    Microsoft.Maui.Controls.Handlers.Items.CollectionViewHandler.Mapper.AppendToMapping(
                        "KeyboardAccessibleCollectionView",
                        (handler, view) =>
                        {
                            handler.PlatformView.SingleSelectionFollowsFocus = false;
                        });
#endif
                })
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("SegoeUI-Semibold.ttf", "SegoeSemibold");
                    fonts.AddFont("FluentSystemIcons-Regular.ttf", FluentUI.FontFamily);
                });

#if DEBUG
            builder.Logging.AddDebug();
            builder.Services.AddLogging(configure => configure.AddDebug());
#endif

            builder.Services.AddSingleton(new HttpClient());
            builder.Services.AddSingleton<WeatherAPIService>();
            builder.Services.AddSingleton<WeatherSearchService>();
            builder.Services.AddTransient<WeatherStatisticsService>();

            builder.Services.AddSingleton<WeatherHistoryRepository>();
            builder.Services.AddSingleton<WeatherStatisticsRepository>();
            builder.Services.AddSingleton<FavoritesRepository>();

            builder.Services.AddTransient<WeatherApp.Pages.HomePage>();
            builder.Services.AddTransient<WeatherApp.Views.CurrentWeatherPage>();
            builder.Services.AddTransient<WeatherApp.Views.ForecastPage>();
            builder.Services.AddTransient<WeatherApp.Views.AirQualityPage>();
            builder.Services.AddTransient<WeatherApp.Views.HistoryPage>();
            builder.Services.AddTransient<WeatherApp.Views.FavoritesPage>();
            builder.Services.AddTransient<WeatherApp.Views.ComparePage>();
            builder.Services.AddTransient<WeatherApp.Views.StatisticsPage>();

            return builder.Build();
        }
    }
}
