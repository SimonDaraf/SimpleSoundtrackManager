using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class OpenSessionViewModel : ObservableObject
    {
        private readonly SessionManager sessionManager;
        private readonly NavigationService navigationService;

        public Window? Owner { get; set; }

        [ObservableProperty]
        private ObservableCollection<SessionSelectorViewModel> sessions;

        public OpenSessionViewModel(SessionManager sessionManager, NavigationService navigationService, Func<SessionSelectorViewModel> sessionSelectorFactory)
        {
            this.sessionManager = sessionManager;
            this.navigationService = navigationService;

            navigationService.OnNavigationRequested += NavigationService_OnNavigationRequested;

            sessions = new ObservableCollection<SessionSelectorViewModel>();
            foreach (Session session in sessionManager.GetSessionsInStandardDirectory())
            {
                SessionSelectorViewModel vm = sessionSelectorFactory();
                vm.Session = session;
                sessions.Add(vm);
            }
        }

        private void NavigationService_OnNavigationRequested(object? sender, ObservableObject e)
        {
            navigationService.OnNavigationRequested -= NavigationService_OnNavigationRequested;
            Owner?.Close();
        }

        [RelayCommand]
        private void CreateNewSession(Window current)
        {
            Window createWindow = navigationService.GetCreateNewWindow();
            current.Close();
            createWindow.Owner = App.Current.MainWindow;
            createWindow.ShowDialog();
        }

        [RelayCommand]
        private void BrowseSession(Window window)
        {
            Session? session = sessionManager.BrowseSession();

            if (session == null)
            {
                return;
            }

            navigationService.RequestNavigationToSession(session);
        }
    }
}
