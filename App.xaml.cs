using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Services;
using SimpleSoundtrackManager.MVVM.View;
using SimpleSoundtrackManager.MVVM.ViewModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;
using SimpleSoundtrackManager.MVVM.Model;

namespace SimpleSoundtrackManager
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

            services.AddTransient(provider => new PreferencesWindow
            {
                DataContext = provider.GetRequiredService<PreferencesViewModel>()
            });

            services.AddTransient(provider =>
            {
                OpenSessionViewModel vm = provider.GetRequiredService<OpenSessionViewModel>();
                OpenSessionWindow v = new OpenSessionWindow
                {
                    DataContext = vm
                };
                vm.Owner = v;
                return v;
            });

            services.AddTransient(provider => new CreateNewSessionWindow
            {
                DataContext = provider.GetRequiredService<CreateNewSessionViewModel>()
            });

            // Declaration of all view models.
            services.AddSingleton<MainWindowViewModel>();
            services.AddTransient<OpenSessionViewModel>();
            services.AddTransient<CreateNewSessionViewModel>();
            services.AddTransient<SessionSelectorViewModel>();
            services.AddSingleton<PreferencesViewModel>();
            services.AddKeyedTransient<NavigatableViewModel, ActiveSessionViewModel>(NavigationViews.ActiveSession);
            services.AddKeyedTransient<NavigatableViewModel, SessionViewModel>(NavigationViews.SessionView);

            // Declaration of all services.
            services.AddSingleton<NavigationService>();
            services.AddSingleton<SessionManager>();
            services.AddSingleton<SessionTracker>();
            services.AddSingleton<AudioPlayer>();
            services.AddSingleton<SettingsManager>();

            // Other misc declarations.
            services.AddTransient<Func<NavigationViews, NavigatableViewModel>>(provider => key => provider.GetRequiredKeyedService<NavigatableViewModel>(key));
            services.AddTransient<Func<CreateNewSessionWindow>>(provider => () => provider.GetRequiredService<CreateNewSessionWindow>());
            services.AddTransient<Func<SessionSelectorViewModel>>(provider => () => provider.GetRequiredService<SessionSelectorViewModel>());
            services.AddTransient<Func<PreferencesWindow>>(provider => () => provider.GetRequiredService<PreferencesWindow>());

            // Avoid having the container track these instances.
            services.AddTransient<Func<Track, TrackSelectorViewModel>>(provider => {
                AudioPlayer audioPlayer = provider.GetRequiredService<AudioPlayer>();
                SessionManager sessionManager = provider.GetRequiredService<SessionManager>();
                SessionTracker sessionTracker = provider.GetRequiredService<SessionTracker>();
                return track =>
                {
                    TrackSelectorViewModel vm = new TrackSelectorViewModel(sessionManager, sessionTracker, audioPlayer);
                    vm.Track = track;
                    return vm;
                };
            });
            services.AddTransient<Func<TrackSelectorViewModel>>(provider =>
            {
                AudioPlayer audioPlayer = provider.GetRequiredService<AudioPlayer>();
                SessionManager sessionManager = provider.GetRequiredService<SessionManager>();
                SessionTracker sessionTracker = provider.GetRequiredService<SessionTracker>();
                return () => new TrackSelectorViewModel(sessionManager, sessionTracker, audioPlayer);
            });

            // Build provider.
            serviceProvider = services.BuildServiceProvider();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            serviceProvider.GetRequiredService<MainWindow>();
            MainWindow.Show();

            base.OnStartup(e);

            // Show open group window on startup.
            OpenSessionWindow gw = serviceProvider.GetRequiredService<OpenSessionWindow>();
            gw.Owner = MainWindow;
            gw.ShowDialog();
        }
    }
}
