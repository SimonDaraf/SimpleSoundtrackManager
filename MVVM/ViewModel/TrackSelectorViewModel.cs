using SimpleSoundtrackManager.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model.Services;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class TrackSelectorViewModel : ObservableObject
    {
        private readonly NavigationService navigationService;

        [ObservableProperty]
        private Track? track;

        public TrackSelectorViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        [RelayCommand]
        private void Open()
        {
            if (Track is null) return;

            navigationService.RequestNavigationToTrack(Track);
        }
    }
}
