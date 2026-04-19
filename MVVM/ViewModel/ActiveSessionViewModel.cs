using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleSoundtrackManager.MVVM.Model;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using System.Collections.ObjectModel;
using System.Windows.Forms;

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

        [ObservableProperty]
        private string status = $"Loading...";

        [ObservableProperty]
        private float volume = 1;

        public ActiveSessionViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        partial void OnVolumeChanged(float value)
        {
            if (mixer is null)
                return;

            mixer.SetVolume(value);

            if (Session is not null)
                Session.Volume = value;
        }

        public override void OnNavigation()
        {
            if (Session is null)
                throw new Exception("Session needs to be valid before navigating.");

            Volume = Session.Volume;

            foreach (Track track in Session.Tracks)
            {
                TrackSessionViewModel vm = new TrackSessionViewModel
                {
                    Track = track,
                };
                vm.OnTrackChangeRequested += OnTrackChangeRequested;
                TrackViews.Add(vm);
            }

            Task.Run(() =>
            {
                Status = "Caching Audio Data...";
                mixer = new SessionMixer(Session.Tracks);
                mixer.SetVolume(Volume);
                Status = "Ready";
            });
        }

        private void OnTrackChangeRequested(object? sender, OnTrackChangeRequestedEventArgs e)
        {
            if (mixer is null)
            {
                MessageBox.Show("Still caching audio data, please wait.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }   

            if (sender is not null && sender is TrackSessionViewModel vm && vm.Track is not null)
            {
                if (e.IsOverlay)
                {
                    //vm.IsActive = true;
                    if (!mixer.IsPlaying)
                        mixer.InitEmpty();

                    if (mixer.IsOverlay(vm.Track))
                    {
                        mixer.RemoveTrackAsOverlay(vm.Track);
                        vm.IsActive = false;
                        vm.State = string.Empty;
                    }
                    else
                    {
                        mixer.AddTrackAsOverlay(vm.Track);
                        vm.IsActive = true;
                        vm.State = "Overlay";
                    }
                }
                else
                {
                    vm.IsActive = true;
                    vm.State = "Base";

                    if (currentActive is not null)
                    {
                        currentActive.IsActive = false;
                        currentActive.State = string.Empty;
                    } 

                    currentActive = vm;
                    if (!mixer.IsPlaying)
                        mixer.Init(vm.Track);
                    else
                        mixer.RequestChange(vm.Track);

                    Status = $"Playing: {vm.Track.Name}";
                }
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

        public override void Cleanup()
        {
            foreach (TrackSessionViewModel vm in TrackViews)
            {
                vm.OnTrackChangeRequested -= OnTrackChangeRequested;
                vm.Dispose();
            }

            mixer?.Dispose();
            GC.Collect();
        }
    }
}
