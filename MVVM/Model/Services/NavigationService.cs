using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.View;
using AutomatedSoundtrackSystem.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace AutomatedSoundtrackSystem.MVVM.Model.Services
{
    public class NavigationService
    {
        private readonly Func<NavigationViews, ObservableObject> tryNavigate;

        public event EventHandler<ObservableObject>? OnNavigationRequested;

        public NavigationService(Func<NavigationViews, ObservableObject> tryNavigate)
        {
            this.tryNavigate = tryNavigate;
        }

        public ObservableObject NavigateToViewModel(NavigationViews view)
        {
            return tryNavigate(view);
        }

        public void RequestNavigationToGroup(Group group)
        {
            if (tryNavigate(NavigationViews.GroupView) is GroupViewModel vm)
            {
                vm.Group = group;
                OnNavigationRequested?.Invoke(this, vm);
            }
        }
    }

    public enum NavigationViews
    {
        GroupView
    }
}
