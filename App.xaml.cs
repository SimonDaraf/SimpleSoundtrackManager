using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.Model.Services;
using AutomatedSoundtrackSystem.MVVM.View;
using AutomatedSoundtrackSystem.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            // Logger
            services.AddLogging(builder =>
            {
                builder.AddFilter("SoundtrackManager", LogLevel.Debug);
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("System", LogLevel.Warning);
            });

            // Declaration of all view and mapping to their data context.
            services.AddSingleton(provider => new MainWindow
            {
                DataContext = provider.GetRequiredService<MainWindowViewModel>()
            });

            services.AddTransient(provider => new OpenGroupWindow
            {
                DataContext = provider.GetRequiredService<OpenGroupViewModel>()
            });

            services.AddTransient(provider => new CreateNewGroupWindow
            {
                DataContext = provider.GetRequiredService<CreateNewGroupViewModel>()
            });

            // Declaration of all view models.
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<TrackSelectorViewModel>();
            services.AddTransient<OpenGroupViewModel>();
            services.AddTransient<CreateNewGroupViewModel>();
            services.AddKeyedTransient<GroupViewModel>(NavigationViews.GroupView);

            // Declaration of all services.
            services.AddSingleton<NavigationService>();
            services.AddSingleton<GroupManager>();

            // Other misc declarations.
            services.AddTransient<Func<Track, TrackSelectorViewModel>>(provider => track => {
                TrackSelectorViewModel vm = provider.GetRequiredService<TrackSelectorViewModel>();
                vm.Track = track;
                return vm;
            });

            services.AddTransient<Func<NavigationViews, ObservableObject>>(provider => key => provider.GetRequiredKeyedService<ObservableObject>(key));
            services.AddTransient<Func<CreateNewGroupWindow>>(provider => () => provider.GetRequiredService<CreateNewGroupWindow>());

            // Build provider.
            serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();

            base.OnStartup(e);

            // Show open group window on startup.
            OpenGroupWindow gw = serviceProvider.GetRequiredService<OpenGroupWindow>();
            gw.Owner = MainWindow;
            gw.ShowDialog();
        }
    }
}
