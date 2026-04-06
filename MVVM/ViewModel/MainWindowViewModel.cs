using AutomatedSoundtrackSystem.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableCollection<TrackSelectorViewModel> tracks;

        public MainWindowViewModel(Func<Track, TrackSelectorViewModel> factory)
        {
            tracks = new ObservableCollection<TrackSelectorViewModel>();

            for (int i = 0; i < 16 ; i++)
            {
                tracks.Add(factory.Invoke(new Track
                {
                    Name = "Långt Namn"
                }));
            }
        }
    }
}
