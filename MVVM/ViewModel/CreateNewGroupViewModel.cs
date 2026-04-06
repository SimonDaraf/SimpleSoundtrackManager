using AutomatedSoundtrackSystem.MVVM.Model.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class CreateNewGroupViewModel : ObservableObject
    {
        private readonly GroupManager groupManager;
        private readonly NavigationService navigationService;

        public CreateNewGroupViewModel(GroupManager groupManager, NavigationService navigationService)
        {
            this.groupManager = groupManager;
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void CreateNewGroup()
        {

        }
    }
}
