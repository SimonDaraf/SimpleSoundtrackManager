using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NavigationService navigationService;

        [ObservableProperty]
        private ObservableObject? activeViewModel;

        public MainWindowViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
            navigationService.OnNavigationRequested += NavigationService_OnNavigationRequested;
        }

        private void NavigationService_OnNavigationRequested(object? sender, ObservableObject e)
        {
            ActiveViewModel = e;
        }
    }
}
