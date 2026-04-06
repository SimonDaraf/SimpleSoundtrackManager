using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class OpenGroupViewModel : ObservableObject
    {
        private readonly GroupManager groupManager;
        private readonly NavigationService navigationService;

        public OpenGroupViewModel(GroupManager groupManager, NavigationService navigationService)
        {
            this.groupManager = groupManager;
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void CreateNewGroup(Window current)
        {
            Window createWindow = navigationService.GetCreateNewWindow();
            current.Close();
            createWindow.Owner = App.Current.MainWindow;
            createWindow.ShowDialog();
        }

        [RelayCommand]
        private void BrowseGroup(Window window)
        {
            Group? group = groupManager.BrowseGroup();

            if (group == null)
            {
                return;
            }

            navigationService.RequestNavigationToGroup(group);
            window.Close();
        }
    }
}
