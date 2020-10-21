using System.ComponentModel;
using PolicyProjectManagementService;

namespace PolicyProjectManagementClient
{
    public class SelectedDevice : DeviceInfoDataContract, INotifyPropertyChanged
    {
        private bool _selected;
        private bool _deviceTaken;
        private bool _needUpdateDevice;
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public delegate void DeviceSelected(long deviceId);

        public DeviceSelected OnDeviceSelected;

        public bool NeedUpdateDevice
        {
            get { return _needUpdateDevice; }
            set
            {
                _needUpdateDevice = value;
                NotifyPropertyChanged(nameof(NeedUpdateDevice));
            }
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = !_deviceTaken && value;

                if (_selected)
                    RaiseOnDeviceSelected(DeviceId);

                NotifyPropertyChanged(nameof(Selected));
            }
        }

        public bool DeviceTaken
        {
            get { return _deviceTaken; }
            set
            {
                _deviceTaken = value;
                NotifyPropertyChanged(nameof(DeviceTaken));
            }
        }

        protected void RaiseOnDeviceSelected(long deviceId)
        {
            var dlg = OnDeviceSelected;
            dlg?.Invoke(deviceId);
        }

        protected void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}