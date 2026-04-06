using AutomatedSoundtrackSystem.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AutomatedSoundtrackSystem.MVVM.ViewModel
{
    public partial class GroupViewModel : ObservableObject
    {
        [ObservableProperty]
        private Group? group;
    }
}
