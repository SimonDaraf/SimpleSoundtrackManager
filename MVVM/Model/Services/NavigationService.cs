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
        private readonly Func<NavigationViews, NavigatableViewModel> tryNavigate;
        private readonly Func<CreateNewSessionWindow> createNewWindowFactory;
        private readonly SessionTracker sessionTracker;

        private Stack<ObservableObject> stack;

        public ObservableObject CurrentView { get => stack.Peek(); }

        public event EventHandler<ObservableObject>? OnNavigationRequested;

        public NavigationService(Func<NavigationViews, NavigatableViewModel> tryNavigate, Func<CreateNewSessionWindow> createNewWindowFactory,
            SessionTracker sessionTracker)
        {
            this.tryNavigate = tryNavigate;
            this.createNewWindowFactory = createNewWindowFactory;
            this.sessionTracker = sessionTracker;
            stack = new Stack<ObservableObject>();
        }

        public Window GetCreateNewWindow()
        {
            return createNewWindowFactory();
        }

        public void RequestNavigationToSession(Session session)
        {
            // A session is the root of all navigation, clear the stack.
            stack.Clear();

            if (tryNavigate(NavigationViews.SessionView) is SessionViewModel vm)
            {
                sessionTracker.SetActiveSession(session);
                vm.Session = session;
                stack.Push(vm);
                vm.OnNavigation();
                OnNavigationRequested?.Invoke(this, vm);
            }
        }

        public void TryPopViewFromStack()
        {
            stack.Pop();
            OnNavigationRequested?.Invoke(this, stack.Peek());
        }
    }

    public enum NavigationViews
    {
        SessionView,
    }
}
