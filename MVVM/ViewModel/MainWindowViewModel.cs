using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using SimpleSoundtrackManager.MVVM.Model.Utils;
using System.Windows;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NavigationService navigationService;
        private readonly SessionManager sessionManager;
        private readonly SessionTracker sessionTracker;

        [ObservableProperty]
        private ObservableObject? activeViewModel;

        public MainWindowViewModel(NavigationService navigationService, SessionManager sessionManager, SessionTracker sessionTracker)
        {
            this.navigationService = navigationService;
            this.sessionManager = sessionManager;
            this.sessionTracker = sessionTracker;
            navigationService.OnNavigationRequested += NavigationService_OnNavigationRequested;
        }

        private void NavigationService_OnNavigationRequested(object? sender, ObservableObject e)
        {
            ActiveViewModel = e;
        }

        [RelayCommand]
        private void OpenSession()
        {
            MessageBoxResult result = TrySaveChanges();
            if (result == MessageBoxResult.Cancel) return;

            Session? session = sessionManager.BrowseSession();

            if (session is null)
                return;

            navigationService.RequestNavigationToSession(session);
        }

        [RelayCommand]
        private void NewSession()
        {
            MessageBoxResult result = TrySaveChanges();
            if (result == MessageBoxResult.Cancel) return;

            Window w = navigationService.GetCreateNewWindow();
            w.Owner = App.Current.MainWindow;
            w.ShowDialog();
        }

        [RelayCommand]
        private void SaveSession()
        {

        }

        [RelayCommand]
        private void SaveSessionAs()
        {
            Session? session = sessionTracker.ActiveSession;
            if (session is not null && session.IsDirty)
            {
                Serializer.ToBinary(session, session.FullPath);
            }
        }

        private MessageBoxResult TrySaveChanges()
        {
            Session? session = sessionTracker.ActiveSession;

            if (session is not null && session.IsDirty)
            {
                MessageBoxResult res = MessageBox.Show("You have unsaved changes, do you want to save changes before exiting?", "Warning",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (res == MessageBoxResult.Yes)
                {
                    Serializer.ToBinary(session, session.FullPath);
                }

                return res;
            }

            return MessageBoxResult.None;
        }
    }
}
