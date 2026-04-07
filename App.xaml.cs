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
            services.AddTransient<TrackSelectorViewModel>();
            services.AddTransient<OpenSessionViewModel>();
            services.AddTransient<CreateNewSessionViewModel>();
            services.AddTransient<SessionSelectorViewModel>();
            services.AddKeyedTransient<NavigatableViewModel, SessionViewModel>(NavigationViews.SessionView);
            services.AddKeyedTransient<NavigatableViewModel, TrackEditorViewModel>(NavigationViews.TrackView);

            // Declaration of all services.
            services.AddSingleton<NavigationService>();
            services.AddSingleton<SessionManager>();
            services.AddSingleton<SessionTracker>();

            // Other misc declarations.
            services.AddTransient<Func<Track, TrackSelectorViewModel>>(provider => track => {
                TrackSelectorViewModel vm = provider.GetRequiredService<TrackSelectorViewModel>();
                vm.Track = track;
                return vm;
            });

            services.AddTransient<Func<NavigationViews, NavigatableViewModel>>(provider => key => provider.GetRequiredKeyedService<NavigatableViewModel>(key));
            services.AddTransient<Func<CreateNewSessionWindow>>(provider => () => provider.GetRequiredService<CreateNewSessionWindow>());
            services.AddTransient<Func<SessionSelectorViewModel>>(provider => () => provider.GetRequiredService<SessionSelectorViewModel>());
            services.AddTransient<Func<TrackSelectorViewModel>>(provider => () => provider.GetRequiredService<TrackSelectorViewModel>());

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
