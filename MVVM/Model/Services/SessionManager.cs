using SimpleSoundtrackManager.MVVM.Model.Data;
using SimpleSoundtrackManager.MVVM.Model.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
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
            directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SoundtrackManager");
            ValidateDirectoryPath();
        }

        private void ValidateDirectoryPath()
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private Session? LoadGroup(string path)
        {
            try
            {
                return Serializer.Deserialize<Session>(path);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to load Session:", [ex]);
                return null;
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
                .GetFiles("*.sm", SearchOption.AllDirectories)
                .OrderBy(f => f.LastWriteTime)];

            List<Session> sessions = [];

            foreach (FileInfo fileInfo in files)
            {
                try
                {
                    Session session = Serializer.Deserialize<Session>(fileInfo.FullName);
                    session.LastModified = fileInfo.LastWriteTime.ToShortDateString();
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

            string finalPath = Path.Combine(path, $"{name}.sm");

            Session group = new Session
            {
                Name = name,
                Path = finalPath
            };

            Serializer.ToBinary(group, finalPath);
            return group;
        }

        /// <summary>
        /// Opens file explorer and opens the selected group.
        /// </summary>
        public Session? BrowseSession()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "SM Session Files (*.sm)|*.sm";
            openFileDialog.InitialDirectory = directory;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                return LoadGroup(openFileDialog.FileName);
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
    }
}
