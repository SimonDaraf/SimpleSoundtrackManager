using AutomatedSoundtrackSystem.MVVM.View;
using AutomatedSoundtrackSystem.MVVM.ViewModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace AutomatedSoundtrackSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly IServiceProvider serviceProvider;

        public App()
        {
            IServiceCollection services = new ServiceCollection();

            // Declaration of all view and mapping to their data context.
            services.AddSingleton(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>()
            });

            // Declaration of all view models.
            services.AddSingleton<MainWindowViewModel>();

            // Declaration of all services.

            // Other misc declarations.

            // Build provider.
            serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            serviceProvider.GetRequiredService<MainWindow>();

            MainWindow.Show();
            base.OnStartup(e);
        }
    }
}
