using AutomatedSoundtrackSystem.MVVM.View;
using System.Windows;

namespace AutomatedSoundtrackSystem.MVVM.Model.Services
{
    public class NavigationService
    {
        private Func<Window> openGroupWindow;

        public NavigationService(Func<OpenGroupWindow> openGroupWindow)
        {
            this.openGroupWindow = openGroupWindow;
        }

        public Window OpenGroupWindow()
        {
            return openGroupWindow.Invoke();
        }
    }
}
