using AutomatedSoundtrackSystem.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class TrackSelectorViewModel : ObservableObject
    {
        [ObservableProperty]
        private Track? track;
    }
}
