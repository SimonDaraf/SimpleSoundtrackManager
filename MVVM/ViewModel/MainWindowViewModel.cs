using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NavigationService navigationService;
        private readonly SessionManager sessionManager;

        [ObservableProperty]
        private ObservableObject? activeViewModel;

        public MainWindowViewModel(NavigationService navigationService, SessionManager sessionManager)
        {
            this.navigationService = navigationService;
            this.sessionManager = sessionManager;
            navigationService.OnNavigationRequested += NavigationService_OnNavigationRequested;
        }

        private void NavigationService_OnNavigationRequested(object? sender, ObservableObject e)
        {
            ActiveViewModel = e;
        }

        [RelayCommand]
        private void OpenSession()
        {
            Session? session = sessionManager.BrowseSession();

            if (session is null)
                return;

            navigationService.RequestNavigationToSession(session);
        }

        [RelayCommand]
        private void NewSession()
        {
            Window w = navigationService.GetCreateNewWindow();
            w.Owner = App.Current.MainWindow;
            w.ShowDialog();
        }
    }
}
