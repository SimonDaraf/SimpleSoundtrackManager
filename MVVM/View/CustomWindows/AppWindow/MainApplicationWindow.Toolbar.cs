using SimpleSoundtrackManager.MVVM.Model.Utils;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SimpleSoundtrackManager.MVVM.View.CustomWindows.AppWindow
{
    public partial class MainApplicationWindow : ApplicationWindow
    {
        /// <summary>
        /// <c>Method</c> Opens the system context menu.
        /// </summary>
        private void OpenSystemContextMenu()
        {
            Point position = new(0, 30);
            Point screen = PointToScreen(position);

            IntPtr handle = new WindowInteropHelper(this).Handle;
            IntPtr systemMenu = NativeUtils.GetSystemMenu(handle, false);

            if (WindowState != WindowState.Maximized)
            {
                NativeUtils.EnableMenuItem(systemMenu, 61488, 0);
            }
            else
            {
                NativeUtils.EnableMenuItem(systemMenu, 61488, 1);
            }

            int num1 = NativeUtils.TrackPopupMenuEx(systemMenu,
                 NativeUtils.TPM_LEFTALIGN | NativeUtils.TPM_RETURNCMD,
                 Convert.ToInt32(screen.X + 2), Convert.ToInt32(screen.Y + 2),
                 handle, IntPtr.Zero);

            if (num1 == 0)
            {
                return;
            }

            NativeUtils.PostMessage(handle, 274, new IntPtr(num1), IntPtr.Zero);
        }

        /// <summary>
        /// <c>Method</c> Handles when the user clicks on the ApplicationLogo.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Data related to the mouse button event.</param>
        private void OnApplicationLogoMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // If the user clicks more than one time on the application logo.
            // According to default windows default behaviour the application should close.
            if (e.ClickCount != 2)
            {
                OpenSystemContextMenu();
            }
            else
            {
                e.Handled = true;
                Close();
            }
        }
    }
}
