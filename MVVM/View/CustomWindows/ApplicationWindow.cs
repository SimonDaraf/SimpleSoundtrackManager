using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleSoundtrackManager.MVVM.View.CustomWindows
{
    public abstract class ApplicationWindow : Window
    {
        /// Element References.
        internal Button? ButtonCross { get; set; }
        internal Grid? GridControl { get; set; }

        /// <summary>
        /// <c>Method</c> Returns a template child based on given child name.
        /// </summary>
        /// <typeparam name="T">Generic that implements DependencyObject</typeparam>
        /// <param name="childName">The name of the template child.</param>
        /// <returns>The template child.</returns>
        public T GetRequiredTemplateChild<T>(string childName) where T : DependencyObject
        {
            return (T)GetTemplateChild(childName);
        }

        /// <summary>
        /// Handles when the user clicks on the ControlPanel.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Data related to the mouse button event.</param>
        public abstract void OnControlPanelMouseLeftButtonDown(object sender, MouseButtonEventArgs e);

        /// <summary>
        /// Handles when the user clicks on cross button.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Data related to the click event.</param>
        public abstract void ButtonCross_Click(object sender, RoutedEventArgs e);
    }
}
