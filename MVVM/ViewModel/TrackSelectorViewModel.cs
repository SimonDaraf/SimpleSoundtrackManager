using SimpleSoundtrackManager.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model.Services;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class TrackSelectorViewModel : ObservableObject
    {
        private readonly NavigationService navigationService;
        private readonly SessionTracker sessionTracker;

        [ObservableProperty]
        private Track? track;

        public TrackSelectorViewModel(NavigationService navigationService, SessionTracker sessionTracker)
        {
            this.navigationService = navigationService;
            this.sessionTracker = sessionTracker;
        }

        [RelayCommand]
        private void Delete()
        {
            if (Track is null) return;
            sessionTracker.RemoveTrackFromActiveSesion(Track);
        }
    }
}
