using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.Windows;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class CreateNewSessionViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly SessionManager sessionManager;
        private readonly NavigationService navigationService;

        [ObservableProperty]
        private string name = "MySession";

        [ObservableProperty]
        private string path = string.Empty;

        public CreateNewSessionViewModel(ILogger<CreateNewSessionViewModel> logger, SessionManager sessionManager, NavigationService navigationService)
        {
            this.logger = logger;
            this.sessionManager = sessionManager;
            this.navigationService = navigationService;

            path = sessionManager.GetStandardPath();
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window.Close();
        }

        [RelayCommand]
        private void CreateNewSession(Window window)
        {
            try
            {
                Session? session = sessionManager.CreateNewSession(Name);

                if (session is null)
                {
                    return;
                }

                navigationService.RequestNavigationToSession(session);
                window.Close();
            } catch (Exception ex)
            {
                logger.LogError("An error occurred when creating new session", [ex]);
                MessageBox.Show("Failed to create new session.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void BrowseForCustomPath()
        {
            OpenFolderDialog openFolderDialog = new OpenFolderDialog();
            openFolderDialog.Multiselect = false;
            openFolderDialog.Title = "Select Custom Location";
            openFolderDialog.InitialDirectory = Path;

            bool? result = openFolderDialog.ShowDialog();

            if (result == true)
            {
                Path = openFolderDialog.FolderName;
            } else if (result is null)
            {
                MessageBox.Show("Failed to select custom location.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
