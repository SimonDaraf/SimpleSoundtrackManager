using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class SessionSelectorViewModel : ObservableObject
    {
        private readonly NavigationService navigationService;

        [ObservableProperty]
        private Session? session;

        public SessionSelectorViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void NavigateToSession()
        {
            if (Session is null)
                return;
            navigationService.RequestNavigationToSession(Session);
        }
    }
}
