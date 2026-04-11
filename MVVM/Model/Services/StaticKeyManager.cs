using System.Windows;
using System.Windows.Input;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class StaticKeyManager
    {
        public static event EventHandler<Key>? OnKeyDown;
        public static event EventHandler<Key>? OnKeyUp;

        private static Window? Window { get; set; }

        public static void DeRegisterActiveWindow()
        {
            if (Window == null) return;
            Window.PreviewKeyDown -= Window_PreviewKeyDown;
            Window.PreviewKeyUp -= Window_PreviewKeyUp;
        }

        public static void RegisterWindowComponent(Window window)
        {
            Window = window;
            Window.PreviewKeyDown += Window_PreviewKeyDown;
            Window.PreviewKeyUp += Window_PreviewKeyUp;
        }

        private static void Window_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            OnKeyUp?.Invoke(Window, e.Key);
        }

        private static void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            OnKeyDown?.Invoke(Window, e.Key);
        }
    }
}
