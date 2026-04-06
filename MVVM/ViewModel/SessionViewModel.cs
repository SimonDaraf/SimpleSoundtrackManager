using SimpleSoundtrackManager.MVVM.Model.Data;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class SessionViewModel : ObservableObject
    {
        [ObservableProperty]
        private Session? session;
    }
}
