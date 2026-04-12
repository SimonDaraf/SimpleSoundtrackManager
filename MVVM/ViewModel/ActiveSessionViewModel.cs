using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using System.Collections.ObjectModel;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class ActiveSessionViewModel : NavigatableViewModel
    {
        private readonly NavigationService navigationService;

        [ObservableProperty]
        private Session? session;

        [ObservableProperty]
        private ObservableCollection<TrackSessionViewModel> trackViews = [];

        public ActiveSessionViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public override void OnNavigation()
        {
            if (Session is null)
                throw new Exception("Session needs to be valid before navigating.");

            foreach (Track track in Session.Tracks)
            {
                TrackViews.Add(new TrackSessionViewModel
                {
                    Track = track,
                });
            }
        }

        [RelayCommand]
        private void EndSession()
        {
            if (Session is null) return;
            navigationService.RequestNavigationToSession(Session);
        }
    }
}
