using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Utils;
using System.IO;
using System.Windows;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class SessionManager
    {
        private readonly string directory;
        private readonly ILogger logger;

        public SessionManager(ILogger<SessionManager> logger)
        {
            this.logger = logger;
            directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleSoundtrackManager");
            ValidateDirectoryPath();
        }

        private void ValidateDirectoryPath()
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private Session? LoadSession(string path)
        {
            try
            {
                Session session = Serializer.Deserialize<Session>(path);
                return session;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to load Session:", [ex]);
                return null;
            }
        }

        private bool IsSessionCorrupt(Session session, out Track[] corrupt)
        {
            List<Track> c = new List<Track>();
            foreach (Track t in session.Tracks)
            {
                if (!File.Exists(t.FilePath))
                {
                    c.Add(t);
                }
            } 

            corrupt = [..c];
            return corrupt.Length > 0;
        }

        private bool TryRecoverLostAudioFile(Session session, Track track)
        {
            string audioDirectory = Path.GetDirectoryName(track.FilePath) ?? throw new Exception("Invalid path state");
            if (!Directory.Exists(audioDirectory))
            {
                audioDirectory = Path.Combine(session.DirectoryPath, "AudioFiles");
                Directory.CreateDirectory(audioDirectory);
            }
            
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = $"Recover {track.Name}";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Audio Files (*.mp3, *.wav)|*.mp3;*.wav";
            openFileDialog.InitialDirectory = audioDirectory;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                // Most problems come from someone trying to open a session created on another computer.
                if (File.Exists(track.FilePath))
                {
                    File.Delete(track.FilePath);
                }

                string selectedPath = Path.GetDirectoryName(openFileDialog.FileName) ?? throw new Exception("Invalid path state");
                string finalPath = openFileDialog.FileName;

                if (audioDirectory != selectedPath)
                {
                    finalPath = Path.Combine(audioDirectory, openFileDialog.SafeFileName);
                    File.Move(openFileDialog.FileName, finalPath);
                }

                (long length, long ms) = GetTrackLengthFromfile(finalPath);
                track.TrackLength = length;
                track.LengthInMs = ms;
                track.FilePath = finalPath;
                track.LoopPoint = length;
                track.TrackVolume = 0.5f;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Validate that a session is not corrup.
        /// If it is, attempt recovery.
        /// </summary>
        public void ValidateSession(Session session)
        {
            if (IsSessionCorrupt(session, out Track[] corrupt))
            {
                MessageBoxResult res = MessageBox.Show($"Found {corrupt.Length} missing audio files in session, do you wish to try to recover these? If not they will be removed from the session",
                    "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);

                if (res == MessageBoxResult.Yes)
                {
                    foreach (Track t in corrupt)
                    {
                        if (!TryRecoverLostAudioFile(session, t))
                        {
                            session.Tracks.Remove(t);
                        }
                    }
                }
                else
                {
                    foreach (Track t in corrupt)
                    {
                        session.Tracks.Remove(t);
                    }
                }
                // Finally save.
                Serializer.ToBinary(session, session.FullPath);
            }
        }

        /// <summary>
        /// Returns the standard directory.
        /// </summary>
        public string GetStandardPath()
        {
            return directory;
        }

        /// <summary>
        /// Access all groups in standard directory.
        /// </summary>
        public List<Session> GetSessionsInStandardDirectory()
        {
            // Get all recent files in base directory.
            FileInfo[] files = [.. new DirectoryInfo(directory)
                .GetFiles("*.ssm", SearchOption.AllDirectories)
                .OrderBy(f => f.LastWriteTime)];

            List<Session> sessions = [];

            foreach (FileInfo fileInfo in files)
            {
                try
                {
                    Session session = Serializer.Deserialize<Session>(fileInfo.FullName);
                    session.LastModified = fileInfo.LastWriteTime.ToShortDateString();
                    if (!File.Exists(session.FullPath))
                    {
                        // Attempt path recovery.
                        string dir = Path.GetDirectoryName(fileInfo.FullName) ?? throw new Exception("Invalid file path state.");
                        session.DirectoryPath = dir;
                        session.FullPath = dir;
                    }
                    sessions.Add(session);
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to load file, exception caught:", [ex]);
                }
            }

            return sessions;
        }

        /// <summary>
        /// Tries to create a new group and returns the created group.
        /// </summary>
        public Session? CreateNewSession(string name)
        {
            string path = Path.Combine(directory, name);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            else
            {
                MessageBoxResult res = MessageBox.Show("Session already exists, are you sure you want to override it?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (res == MessageBoxResult.No)
                {
                    return null;
                }
            }

            string finalPath = Path.Combine(path, $"{name}.ssm");
            Session session = new Session
            {
                Name = name,
                FullPath = finalPath,
                DirectoryPath = Path.GetDirectoryName(finalPath) ?? throw new Exception("Invalid file path state.")
            };

            Serializer.ToBinary(session, finalPath);
            return session;
        }

        /// <summary>
        /// Opens file explorer and opens the selected group.
        /// </summary>
        public Session? BrowseSession()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "SSM Session Files (*.ssm)|*.ssm";
            openFileDialog.InitialDirectory = directory;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                Session? session = LoadSession(openFileDialog.FileName);
                if (session is null) return session; // Dont continue.

                // Make sure all paths are up to date internally.
                session.FullPath = openFileDialog.FileName;
                session.DirectoryPath = Path.GetDirectoryName(openFileDialog.FileName) ?? throw new Exception("Invalid file path state.");
                return session;
            } else if (result == false)
            {
                return null;
            } else
            {
                logger.LogWarning("Failed to load session file.");
                MessageBox.Show("Failed to load session.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public void ReplaceAudioFileInTrack(Track track)
        {
            string audioDirectory = Path.GetDirectoryName(track.FilePath) ?? throw new Exception("Invalid path state");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Audio Files (*.mp3, *.wav)|*.mp3;*.wav";
            openFileDialog.InitialDirectory = audioDirectory;

            bool? result = openFileDialog.ShowDialog();

            if (result == true) {
                File.Delete(track.FilePath);

                string selectedPath = Path.GetDirectoryName(openFileDialog.FileName) ?? throw new Exception("Invalid path state");

                string finalPath = openFileDialog.FileName;

                if (audioDirectory != selectedPath)
                {
                    finalPath = Path.Combine(audioDirectory, openFileDialog.SafeFileName);
                    File.Move(openFileDialog.FileName, finalPath);
                }

                (long length, long ms) = GetTrackLengthFromfile(finalPath);
                track.TrackLength = length;
                track.LengthInMs = ms;
                track.FilePath = finalPath;
                track.LoopPoint = length;
                track.TrackVolume = 0.5f;
            }
        }

        public Track? CreateNewTrack(Session session)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "Audio Files (*.mp3, *.wav)|*.mp3;*.wav";
            openFileDialog.InitialDirectory = session.DirectoryPath;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string audioDirectory = Path.Combine(session.DirectoryPath, "AudioFiles");
                string selectedPath = Path.GetDirectoryName(openFileDialog.FileName) ?? throw new Exception("Invalid path state");

                (long length, long ms) = GetTrackLengthFromfile(openFileDialog.FileName);

                // If audio file is already in correct directory. No need to copy it.
                if (audioDirectory == selectedPath)
                {
                    Track track = new Track
                    {
                        Name = openFileDialog.SafeFileName,
                        FilePath = openFileDialog.FileName,
                        LoopPoint = length,
                        TrackLength = length,
                        LengthInMs = ms,
                        TrackVolume = 0.5f,
                    };
                    return track;
                }
                else
                {
                    if (!Directory.Exists(audioDirectory))
                    {
                        Directory.CreateDirectory(audioDirectory);
                    }

                    string destPath = Path.Combine(audioDirectory, openFileDialog.SafeFileName);

                    if (File.Exists(destPath))
                    {
                        int num = 0;
                        string fixedPath;
                        string dir = Path.GetDirectoryName(destPath) ?? throw new Exception("Invalid path state");
                        string name = Path.GetFileNameWithoutExtension(destPath);
                        string ext = Path.GetExtension(destPath);
                        do
                        {
                            fixedPath = Path.Combine(dir, $"{name}_{num}{ext}");
                            num++;
                        } while (File.Exists(fixedPath));
                        destPath = fixedPath;
                    }

                    File.Copy(openFileDialog.FileName, destPath);

                    Track track = new Track
                    {
                        Name = openFileDialog.SafeFileName,
                        FilePath = destPath,
                        LoopPoint = length,
                        TrackLength = length,
                        LengthInMs = ms,
                        TrackVolume = 0.5f,
                    };
                    return track;
                }
            } if (result is null)
            {
                logger.LogWarning("Failed to open audio file.");
                MessageBox.Show("Failed to open audio file.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            } else
            {
                return null;
            }
        }

        private (long, long) GetTrackLengthFromfile(string filePath)
        {
            using AudioFileReader audio = new AudioFileReader(filePath);
            return (audio.Length, (long)audio.TotalTime.TotalMilliseconds);
        }
    }
}
