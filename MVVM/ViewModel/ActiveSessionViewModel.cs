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
        private readonly Func<SessionMixer> mixerFactory;

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

        [ObservableProperty]
        private ObservableCollection<string> filters;

        [ObservableProperty]
        private string activeFilter = "All";

        [ObservableProperty]
        private bool showActive = false;

        private List<TrackSessionViewModel> allTrackViews;

        public ActiveSessionViewModel(NavigationService navigationService, Func<SessionMixer> mixerFactory)
        {
            this.navigationService = navigationService;
            this.mixerFactory = mixerFactory;
            allTrackViews = new List<TrackSessionViewModel>();
            filters = new ObservableCollection<string>();
            Filters.Add("All");
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
                allTrackViews.Add(vm);

                string category = vm.Track.Category;
                if (!Filters.Contains(category))
                    Filters.Add(category);
            }

            Filter();

            Task.Run(() =>
            {
                Status = "Caching Audio Data...";
                SessionMixer m = mixerFactory();
                m.CacheAudioData(Session.Tracks);
                m.SetVolume(Volume);
                mixer = m;
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

                    if (currentActive is not null && currentActive.Equals(vm))
                        currentActive = null;

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

                    Status = mixer.IsPlaying ? "Playing" : "Ready";
                }
            }

            Filter();
        }

        partial void OnActiveFilterChanged(string value)
        {
            Filter();
        }

        partial void OnShowActiveChanged(bool value)
        {
            Filter();
        }

        private void Filter()
        {
            TrackViews.Clear();

            foreach (TrackSessionViewModel vm in allTrackViews)
            {
                if (vm.Track is null)
                    continue;

                if (ShowActive && !vm.IsActive)
                    continue;

                if (!ActiveFilter.Equals("All") && !vm.Track.Category.Equals(ActiveFilter))
                    continue;

                TrackViews.Add(vm);
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
