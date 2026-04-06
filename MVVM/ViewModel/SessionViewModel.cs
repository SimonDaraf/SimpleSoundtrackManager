using AutomatedSoundtrackSystem.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class SessionViewModel : ObservableObject
    {
        [ObservableProperty]
        private Session? session;
    }
}
