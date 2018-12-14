using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TowerDefense
{
    public class BluetoothService : INotifyPropertyChanged
    {
        public IBluetoothModule Bluetooth { get; set; } = DependencyService.Get<IBluetoothModule>();

        private bool _isBTConnected = false;
        public bool IsBTConnected
        {
            get
            {
                return _isBTConnected;
            }
            set
            {
                _isBTConnected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsBTConnected)));
            }
        }

        public BluetoothService(string btDevice)
        {
            Bluetooth.Start(btDevice);
        }

        public void TrackBluetoothConnectionStatus()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    IsBTConnected = Bluetooth.IsConnected();
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
