using CommunityToolkit.Mvvm.ComponentModel;
using NAudio.Wave;
using SimpleSoundtrackManager.MVVM.Model.Services;
using System.Collections.ObjectModel;
using System.Reflection;

namespace SimpleSoundtrackManager.MVVM.ViewModel
{
    public partial class PreferencesViewModel : ObservableObject
    {
        private readonly SettingsManager settingsManager;

        [ObservableProperty]
        private string version = $"v{Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "Unknown Version"}";

        [ObservableProperty]
        private ObservableCollection<string> devices;

        [ObservableProperty]
        private string selectedDevice;

        private int selectedDeviceNumber;

        public PreferencesViewModel(SettingsManager settingsManager)
        {
            this.settingsManager = settingsManager;

            Devices = new ObservableCollection<string>();
            Devices.Add("Default");
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                Devices.Add(WaveOut.GetCapabilities(i).ProductName);
            }

            selectedDeviceNumber = settingsManager.GetSelectedDevice();
            if (selectedDeviceNumber == -1)
                selectedDevice = "Default";
            else
                selectedDevice = devices[selectedDeviceNumber + 1];
        }

        partial void OnSelectedDeviceChanged(string value)
        {
            selectedDeviceNumber = GetNumberOfSelectedDevice(value);
            settingsManager.SetSelectedDevice(selectedDeviceNumber);
            settingsManager.Save();
        }

        private int GetNumberOfSelectedDevice(string selectedDevice)
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                if (WaveOut.GetCapabilities(i).ProductName.Equals(selectedDevice))
                    return i;
            }

            return -1;
        }
    }
}
