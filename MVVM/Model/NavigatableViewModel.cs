using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleSoundtrackManager.MVVM.Model
{
    public abstract class NavigatableViewModel : ObservableObject
    {
        public abstract void OnNavigation();
        public abstract void Cleanup();
    }
}
