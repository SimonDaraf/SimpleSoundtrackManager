using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using System.Collections.ObjectModel;
using System.Printing;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class ActiveSessionViewModel : NavigatableViewModel
    {
        private readonly NavigationService navigationService;

        private TrackSessionViewModel? currentActive;
        private SessionMixer? mixer;

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
                TrackSessionViewModel vm = new TrackSessionViewModel
                {
                    Track = track,
                };
                vm.OnTrackChangeRequested += OnTrackChangeRequested;
                TrackViews.Add(vm);
            }

            mixer = new SessionMixer(Session.Tracks);
        }

        private void OnTrackChangeRequested(object? sender, Track e)
        {
            if (mixer is null)
                return;

            if (sender is not null && sender is TrackSessionViewModel vm && vm.Track is not null)
            {
                vm.IsActive = true;

                if (currentActive is not null)
                    currentActive.IsActive = false;

                currentActive = vm;
                if (!mixer.IsPlaying)
                    mixer.Init(vm.Track);
                else
                    mixer.RequestChange(vm.Track);
            }
        }

        [RelayCommand]
        private void EndSession()
        {
            if (Session is null) return;
            if (mixer is not null)
            {
                mixer.Stop();
            }
            navigationService.RequestNavigationToSession(Session);
        }
    }
}
