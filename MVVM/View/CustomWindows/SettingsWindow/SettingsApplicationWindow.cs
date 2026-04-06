using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleSoundtrackManager.MVVM.View.CustomWindows.SettingsWindow
{
    /// <summary>
    /// <c>Class</c> The settings application window class.
    /// </summary>
    public class SettingsApplicationWindow : ApplicationWindow
    {
        /// <summary>
        /// <c>Method</c> Responsible for assigning a reference to each private field in the class.
        /// Given the static nature of the class the fields should never be null.
        /// Declaring them as nullable is only due to the fact that they are not set in the constructor and instead when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Get element references.
            ButtonCross = GetRequiredTemplateChild<Button>("ButtonCross");
            GridControl = GetRequiredTemplateChild<Grid>("GridControl");

            // Subscribe to events.
            // We first check so thate each button isn't null (Shouldn't be).
            if (ButtonCross != null)
            {
                ButtonCross.Click += ButtonCross_Click;
            }

            // Add eventlistener to ControlBar, also check so it isn't null.
            GridControl?.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnControlPanelMouseLeftButtonDown));

            base.OnApplyTemplate();
        }

        public override void ButtonCross_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public override void OnControlPanelMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
