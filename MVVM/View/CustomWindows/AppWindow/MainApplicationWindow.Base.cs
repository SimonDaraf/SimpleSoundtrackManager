using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AutomatedSoundtrackSystem.MVVM.View.CustomWindows.AppWindow
{
    public partial class MainApplicationWindow : ApplicationWindow
    {
        // Element References
        private Button? ButtonMinimize { get; set; }
        private Button? ButtonRestore { get; set; }
        private Button? ButtonMaximize { get; set; }
        private Grid? GridRoot { get; set; }
        private Image? ApplicationLogo { get; set; }

        // Private fields
        private bool IsMouseDown { get; set; }
        private bool IsDragable { get; set; }
        private Point MousePosition { get; set; }

        /// <summary>
        /// <c>Constructor</c> Constructs an instance of the CustomWindow class.
        /// </summary>
        public MainApplicationWindow()
        {
            // Make sure they're not null.
            IsMouseDown = false;
            IsDragable = false;
            MousePosition = new Point(0, 0);

            // Add handlers to mouse actions.
            // We need some logic to handle these on their own.
            AddHandler(MouseMoveEvent, new MouseEventHandler(OnMouseMove));
            AddHandler(MouseUpEvent, new MouseButtonEventHandler(OnMouseUp));
            AddHandler(MouseDownEvent, new MouseButtonEventHandler(OnMouseDown));
            StateChanged += CustomWindow_StateChanged;
        }

        /// <summary>
        /// <c>Method</c> Responsible for assigning a reference to each private field in the class.
        /// Given the static nature of the class the fields should never be null.
        /// Declaring them as nullable is only due to the fact that they are not set in the constructor and instead when the template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            // Get element references.
            ButtonMinimize = GetRequiredTemplateChild<Button>("ButtonMinimize");
            ButtonRestore = GetRequiredTemplateChild<Button>("ButtonRestore");
            ButtonMaximize = GetRequiredTemplateChild<Button>("ButtonMaximize");
            ButtonCross = GetRequiredTemplateChild<Button>("ButtonCross");
            ApplicationLogo = GetRequiredTemplateChild<Image>("ApplicationLogo");
            GridControl = GetRequiredTemplateChild<Grid>("GridControl");
            GridRoot = GetRequiredTemplateChild<Grid>("GridRoot");

            // Subscribe to events.
            // We first check so thate each button isn't null (Shouldn't be).
            if (ButtonMinimize != null)
            {
                ButtonMinimize.Click += ButtonMinimize_Click;
            }
            if (ButtonRestore != null)
            {
                ButtonRestore.Click += ButtonRestore_Click;
            }
            if (ButtonMaximize != null)
            {
                ButtonMaximize.Click += ButtonMaximize_Click;
            }
            if (ButtonCross != null)
            {
                ButtonCross.Click += ButtonCross_Click;
            }

            // Add event handler to the ApplicationLogo, also check so it isn't null. 
            ApplicationLogo?.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnApplicationLogoMouseLeftButtonDown));

            // Add eventlistener to ControlBar, also check so it isn't null.
            GridControl?.AddHandler(MouseLeftButtonDownEvent, new MouseButtonEventHandler(OnControlPanelMouseLeftButtonDown));

            base.OnApplyTemplate();
        }

        /// <summary>
        /// <c>Method</c> Handles mouse move event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Data related to the mouse event.</param>
        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                // If mouse isn't down, do nothing.
                if (!IsMouseDown || !IsDragable)
                {
                    return;
                }

                if (WindowState == WindowState.Maximized)
                {
                    ToggleWindowState();

                    Left = MousePosition.X - Width / 2;
                    Top = MousePosition.Y - 15;

                    // This is a weird side effect, but by setting conditions to false,
                    // We make sure that DragMove won't be called after draging and directly maximizing the window.
                    if (IsMouseDown && IsDragable)
                    {
                        IsMouseDown = false;
                        IsDragable = false;
                        DragMove();
                    }
                }
            }
            catch (InvalidOperationException)
            {
                IsMouseDown = false;
                IsDragable = false;
            }
        }

        /// <summary>
        /// Handles mouse up event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Data related to the mouse event.</param>
        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            IsMouseDown = false;
            IsDragable = false;
        }

        /// <summary>
        /// Handles mouse down event.
        /// </summary>
        /// <param name="sender">The sender object.</param>
        /// <param name="e">Data related to the mouse event.</param>
        private void OnMouseDown(object sender, MouseEventArgs e)
        {
            IsMouseDown = true;
        }

        public override void OnControlPanelMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Point position = e.GetPosition(this);

            // If the user double clicks, check if window can resize.
            if (e.ClickCount == 2 && ResizeMode == ResizeMode.CanResize)
            {
                ToggleWindowState();
                return;
            }

            // If the window is maximized, one of two conditions is set to allow window to toggle state and move.
            if (WindowState == WindowState.Maximized)
            {
                IsDragable = true;
                MousePosition = position;
            }
            else
            {
                DragMove();
            }
        }

        public override void ButtonCross_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
