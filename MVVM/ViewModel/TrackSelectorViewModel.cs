using SimpleSoundtrackManager.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class TrackSelectorViewModel : ObservableObject
    {
        [ObservableProperty]
        private Track? track;
    }
}
