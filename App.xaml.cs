using AutomatedSoundtrackSystem.MVVM.Model.Data;
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
            services.AddTransient<TrackSelectorViewModel>();

            // Declaration of all services.

            // Other misc declarations.
            services.AddTransient<Func<Track, TrackSelectorViewModel>>(provider => track => {
                TrackSelectorViewModel vm = provider.GetRequiredService<TrackSelectorViewModel>();
                vm.Track = track;
                return vm;
            });

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
