using SimpleSoundtrackManager.MVVM.Model.Services;
using SimpleSoundtrackManager.MVVM.View.CustomWindows.AppWindow;

namespace SimpleSoundtrackManager.MVVM.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MainApplicationWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnDeactivated(EventArgs e)
        {
            StaticKeyManager.DeRegisterActiveWindow();
            base.OnDeactivated(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            StaticKeyManager.RegisterWindowComponent(this);
            base.OnActivated(e);
        }
    }
}