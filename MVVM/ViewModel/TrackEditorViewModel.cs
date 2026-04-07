using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class TrackEditorViewModel : NavigatableViewModel
    {
        private readonly NavigationService navigationService;

        [ObservableProperty]
        private Track? track;

        public TrackEditorViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void OnNavigation()
        {
            return;
        }

        [RelayCommand]
        private void Back()
        {
            navigationService.TryPopViewFromStack();
        }
    }
}
