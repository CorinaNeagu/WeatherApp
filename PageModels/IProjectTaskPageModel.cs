using CommunityToolkit.Mvvm.Input;
using WeatherApp.Models;

namespace WeatherApp.PageModels
{
    public interface IProjectTaskPageModel
    {
        IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
        bool IsBusy { get; }
    }
}