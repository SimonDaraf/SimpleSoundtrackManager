using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using SimpleSoundtrackManager.MVVM.Model.Utils;
using System.Windows;
using System.Windows.Input;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        private readonly NavigationService navigationService;
        private readonly SessionManager sessionManager;
        private readonly SessionTracker sessionTracker;

        [ObservableProperty]
        private ObservableObject? activeViewModel;

        [ObservableProperty]
        private string title = "SSM";

        private bool ctrlHeldDown = false;
        private bool sHeldDown;

        public MainWindowViewModel(NavigationService navigationService, SessionManager sessionManager, SessionTracker sessionTracker)
        {
            this.navigationService = navigationService;
            this.sessionManager = sessionManager;
            this.sessionTracker = sessionTracker;
            navigationService.OnNavigationRequested += NavigationService_OnNavigationRequested;

            sessionTracker.OnBeforeSessionChanged += SessionTracker_OnBeforeSessionChanged;
            sessionTracker.OnSessionOpened += SessionTracker_OnSessionOpened;

            StaticKeyManager.OnKeyDown += StaticKeyManager_OnKeyDown;
            StaticKeyManager.OnKeyUp += StaticKeyManager_OnKeyUp;
        }

        private void StaticKeyManager_OnKeyUp(object? sender, System.Windows.Input.Key e)
        {
            if (e == Key.LeftCtrl) ctrlHeldDown = false;
            if (e == Key.S) sHeldDown = false;
        }

        private void StaticKeyManager_OnKeyDown(object? sender, System.Windows.Input.Key e)
        {
            if (e == Key.LeftCtrl) ctrlHeldDown = true;
            if (e == Key.S) sHeldDown = true;

            if (ctrlHeldDown && sHeldDown)
            {
                SaveSession();
            }
        }

        private void SessionTracker_OnBeforeSessionChanged(object? sender, Session e)
        {
            // Unhook before changing.
            if (sessionTracker.ActiveSession is not null)
            {
                sessionTracker.ActiveSession.OnDirtyStateChanged -= ActiveSession_OnDirtyStateChanged;
            }
        }

        private void SessionTracker_OnSessionOpened(object? sender, Session e)
        {
            if (sessionTracker.ActiveSession is not null)
            {
                sessionTracker.ActiveSession.OnDirtyStateChanged += ActiveSession_OnDirtyStateChanged;
                Title = sessionTracker.ActiveSession.Name;
            }
        }

        private void ActiveSession_OnDirtyStateChanged(object? sender, bool e)
        {
            Title = e ? $"{Title}*" : Title.Replace("*", "");
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
            Session? session = sessionTracker.ActiveSession;
            if (session is not null)
            {
                session.MarkClean();
                Serializer.ToBinary(session, session.FullPath);
            }
        }

        [RelayCommand]
        private void Exit()
        {
            MessageBoxResult res = TrySaveChanges();
            if (res == MessageBoxResult.None || res == MessageBoxResult.Cancel)
                return;
            App.Current.MainWindow.Close();
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
