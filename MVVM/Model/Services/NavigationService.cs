using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.View;
using SimpleSoundtrackManager.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.Windows.Forms;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class NavigationService
    {
        private readonly Func<NavigationViews, ObservableObject> tryNavigate;
        private readonly Func<CreateNewSessionWindow> createNewWindowFactory;

        public event EventHandler<ObservableObject>? OnNavigationRequested;

        public NavigationService(Func<NavigationViews, ObservableObject> tryNavigate, Func<CreateNewSessionWindow> createNewWindowFactory)
        {
            this.tryNavigate = tryNavigate;
            this.createNewWindowFactory = createNewWindowFactory;
        }

        public ObservableObject NavigateToViewModel(NavigationViews view)
        {
            return tryNavigate(view);
        }

        public Window GetCreateNewWindow()
        {
            return createNewWindowFactory();
        }

        public void RequestNavigationToSession(Session session)
        {
            if (tryNavigate(NavigationViews.SessionView) is SessionViewModel vm)
            {
                vm.Session = session;
                OnNavigationRequested?.Invoke(this, vm);
            }
        }
    }

    public enum NavigationViews
    {
        SessionView
    }
}
