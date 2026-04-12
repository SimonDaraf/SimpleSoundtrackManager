using SimpleSoundtrackManager.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model.Services;
using System.Collections.ObjectModel;
using SimpleSoundtrackManager.MVVM.Model;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class SessionViewModel : NavigatableViewModel
    {
        private readonly SessionManager sessionManager;
        private readonly NavigationService navigationService;
        private readonly SessionTracker sessionTracker;
        private readonly Func<TrackSelectorViewModel> selectorFactory;
        private readonly AudioPlayer audioPlayer;

        [ObservableProperty]
        private Session? session;

        [ObservableProperty]
        private ObservableCollection<TrackSelectorViewModel> tracks;

        public SessionViewModel(SessionManager sessionManager, NavigationService navigationService, SessionTracker sessionTracker,
            Func<TrackSelectorViewModel> selectorFactory, AudioPlayer audioPlayer)
        {
            this.sessionManager = sessionManager;
            this.navigationService = navigationService;
            this.sessionTracker = sessionTracker;
            this.selectorFactory = selectorFactory;
            tracks = new ObservableCollection<TrackSelectorViewModel>();
            this.audioPlayer = audioPlayer;

            sessionTracker.OnTrackAdded += SessionTracker_OnTrackAdded;
            sessionTracker.OnTrackRemoved += SessionTracker_OnTrackRemoved;
        }

        private void SessionTracker_OnTrackRemoved(object? sender, Track e)
        {
            foreach (TrackSelectorViewModel vm in Tracks)
            {
                if (vm.Track is not null && vm.Track.Equals(e))
                {
                    Tracks.Remove(vm);
                    return;
                }
            }
        }

        private void SessionTracker_OnTrackAdded(object? sender, Track e)
        {
            TrackSelectorViewModel vm = selectorFactory();
            vm.Track = e;
            Tracks.Add(vm);
        }

        [RelayCommand]
        private void AddNewTrack()
        {
            if (Session == null) return;

            Track? track = sessionManager.CreateNewTrack(Session);

            if (track is not null)
            {
                sessionTracker.AddTrack(track);
            }
        }

        [RelayCommand]
        private void StartSession()
        {
            if (Session == null) return;
            audioPlayer.Stop();
            navigationService.RequestSessionStart(Session);
        }

        public override void OnNavigation()
        {
            if (Session is null) return;
            
            sessionManager.ValidateSession(Session);

            foreach (Track track in Session.Tracks)
            {
                TrackSelectorViewModel vm = selectorFactory();
                vm.Track = track;
                Tracks.Add(vm);
            }
        }
    }
}
