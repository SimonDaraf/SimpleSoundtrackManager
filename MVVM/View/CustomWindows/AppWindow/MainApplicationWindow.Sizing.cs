using System.Windows;

namespace AutomatedSoundtrackSystem.MVVM.View.CustomWindows.AppWindow
{
    public partial class MainApplicationWindow : ApplicationWindow
    {
        /// <summary>
        /// <c>Method</c> Toggles window state based on current window state.
        /// </summary>
        private void ToggleWindowState()
        {
            try
            {
                if (WindowState != WindowState.Maximized)
                {
                    WindowState = WindowState.Maximized;
                }
                else
                {
                    WindowState = WindowState.Normal;
                }
            }
            catch (NullReferenceException e)
            {
                // Incase either is null, display a warning.
                MessageBox.Show(e.Message);
            }
        }

        /// <summary>
        /// <c>Method</c> Sets the margin when the window is maximized.
        /// </summary>
        private void SetMaximizedMargin()
        {
            GridRoot!.Margin = new Thickness(8, 8, 8, 8);
        }

        /// <summary>
        /// <c>Method</c> Sets the margin when the window is not maximized.
        /// </summary>
        private void ResetMaximizedMargin()
        {
            GridRoot!.Margin = new Thickness(0, 0, 0, 0);
        }

        /// <summary>
        /// <c>Method</c> Handles when the WindowState is changed.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Data related to the event argument.</param>
        private void CustomWindow_StateChanged(object? sender, EventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                SetMaximizedMargin();
                ButtonRestore!.Visibility = Visibility.Visible;
                ButtonMaximize!.Visibility = Visibility.Collapsed;
            }
            else
            {
                ResetMaximizedMargin();
                ButtonRestore!.Visibility = Visibility.Collapsed;
                ButtonMaximize!.Visibility = Visibility.Visible;
            }
        }
    }
}
