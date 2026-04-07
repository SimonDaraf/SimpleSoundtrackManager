using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Utils;
using System.Windows;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    /// <summary>
    /// Provides a central session tracker.
    /// </summary>
    public class SessionTracker
    {
        public event EventHandler<Session>? OnBeforeSessionChanged;
        public event EventHandler<Session>? OnSessionOpened;
        public event EventHandler<Track>? OnTrackAdded;

        public Session? ActiveSession { get; private set; }

        public SessionTracker()
        {
            App.Current.MainWindow.Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ActiveSession is not null && ActiveSession.IsDirty)
            {
                MessageBoxResult res = MessageBox.Show("You have unsaved changes, do you want to save changes before exiting?", "Warning", 
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                if (res == MessageBoxResult.Yes)
                {
                    Serializer.ToBinary(ActiveSession, ActiveSession.FullPath);
                } else if (res == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }
        }

        public void SetActiveSession(Session session)
        {
            if (ActiveSession is not null)
            {
                OnBeforeSessionChanged?.Invoke(this, session);
                ActiveSession.Invalidate();
            }

            ActiveSession = session;
            ActiveSession.Initialize();
            OnSessionOpened?.Invoke(this, session);
        }

        public void AddTrack(Track track)
        {
            if (ActiveSession is null)
            {
                throw new Exception("No active session");
            }
            ActiveSession.Tracks.Add(track);
            OnTrackAdded?.Invoke(this, track);
        }
    }
}
