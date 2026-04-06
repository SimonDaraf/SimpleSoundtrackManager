using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class CreateNewGroupViewModel : ObservableObject
    {
        private readonly GroupManager groupManager;
        private readonly NavigationService navigationService;

        [ObservableProperty]
        private string name = string.Empty;

        public CreateNewGroupViewModel(GroupManager groupManager, NavigationService navigationService)
        {
            this.groupManager = groupManager;
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void CreateNewGroup(Window window)
        {
            Group? group = groupManager.CreateNewGroup(Name);

            if (group is null)
            {
                MessageBox.Show("Failed to create new group.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            navigationService.RequestNavigationToGroup(group);
            window.Close();
        }
    }
}
