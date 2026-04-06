using AutomatedSoundtrackSystem.MVVM.Model.Data;
using AutomatedSoundtrackSystem.MVVM.Model.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace AutomatedSoundtrackSystem.MVVM.Model.Services
{
    public class GroupManager
    {
        private readonly string directory;
        private readonly ILogger logger;

        public GroupManager(ILogger<GroupManager> logger)
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

        private Group? LoadGroup(string path)
        {
            try
            {
                return Serializer.Deserialize<Group>(path);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to load group:", [ex]);
                return null;
            }
        }

        /// <summary>
        /// Access all groups in standard directory.
        /// </summary>
        public List<Group> GetGroupsInStandardDirectory()
        {
            // Get all recent files in base directory.
            FileInfo[] files = [.. new DirectoryInfo(directory)
                .GetFiles("*.sm", SearchOption.AllDirectories)
                .OrderBy(f => f.LastWriteTime)];

            List<Group> groups = [];

            foreach (FileInfo fileInfo in files)
            {
                try
                {
                    groups.Add(Serializer.Deserialize<Group>(fileInfo.FullName));
                }
                catch (Exception ex)
                {
                    logger.LogError("Failed to load file, exception caught:", [ex]);
                }
            }

            return groups;
        }

        /// <summary>
        /// Tries to create a new group and returns whether the operation was successful.
        /// </summary>
        public bool CreateNewGroup(string name)
        {
            try
            {
                string path = Path.Combine(directory, name);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string finalPath = Path.Combine(path, $"{name}.sm");

                Group group = new Group
                {
                    Name = name,
                    Path = finalPath
                };

                Serializer.ToBinary(group, finalPath);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to write file.", [ex]);
                return false;
            }
        }

        /// <summary>
        /// Opens file explorer and opens the selected group.
        /// </summary>
        public Group? BrowseGroup()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "SM Group Files (*.sm)|*.sm";
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
                logger.LogWarning("Failed to load group file.");
                MessageBox.Show("Failed to load group.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }
    }
}
