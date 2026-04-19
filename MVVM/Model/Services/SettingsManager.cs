using System.IO;
using System.Text.Json;

namespace SimpleSoundtrackManager.MVVM.Model.Services
{
    public class AppSettings
    {
        public int AudioDevice { get; set; } = -1;
    }

    public class SettingsManager
    {
        private readonly string settingsPath;
        private const string fileName = "settings.json";
        private AppSettings appSettings;

        public SettingsManager()
        {
            settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleSoundtrackManager", "Settings");
            Directory.CreateDirectory(settingsPath);

            settingsPath = Path.Combine(settingsPath, fileName);
            if (!File.Exists(settingsPath))
            {
                appSettings = new AppSettings();
                File.WriteAllText(settingsPath, JsonSerializer.Serialize(appSettings));
            }
            else
            {
                appSettings = JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(settingsPath)) ?? new AppSettings();
            }
        }

        public int GetSelectedDevice() => appSettings.AudioDevice;

        public void SetSelectedDevice(int number) => appSettings.AudioDevice = number;

        public void Save() => File.WriteAllText(settingsPath, JsonSerializer.Serialize(appSettings));
    }
}
