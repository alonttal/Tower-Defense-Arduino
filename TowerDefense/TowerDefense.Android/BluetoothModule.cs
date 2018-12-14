using Android.Bluetooth;
using Java.IO;
using Java.Util;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(TowerDefense.Droid.BluetoothModule))]
namespace TowerDefense.Droid
{
    public class BluetoothModule : IBluetoothModule
    {
        private BluetoothSocket _btSocket = null;
        private CancellationTokenSource _cancellationToken { get; set; } = new CancellationTokenSource();
        private int _threadSleepTime = 1000;
        private InputStreamReader _readerBuffer;
        private OutputStreamWriter _writerBuffer;

        public BluetoothModule() { }

        public void Start(string btDevice)
        {
            Task.Run(async () =>
            {
                BluetoothDevice device = null;
                BluetoothAdapter adapter = null;
                while (!_cancellationToken.IsCancellationRequested)
                {
                    Thread.Sleep(_threadSleepTime);
                    if (!IsConnected())
                    {
                        _threadSleepTime = 1000;
                        try
                        {
                            adapter = BluetoothAdapter.DefaultAdapter;
                            if (adapter == null) System.Diagnostics.Debug.WriteLine("No Bluetooth adapter found.");
                            else System.Diagnostics.Debug.WriteLine("Adapter found...");
                            if (!adapter.IsEnabled) System.Diagnostics.Debug.WriteLine("Bluetooth adapter is not enabled.");
                            else System.Diagnostics.Debug.WriteLine("Adapter enabled...");
                            System.Diagnostics.Debug.WriteLine("Trying to connect to " + btDevice);
                            device = FindDevice(adapter, btDevice);
                            if (device == null) System.Diagnostics.Debug.WriteLine("Named device not found.");
                            else _btSocket = CreateBluetoothSocket(device);
                            if (_btSocket == null) System.Diagnostics.Debug.WriteLine("Bluetooth socket is null.");
                            else
                            {
                                await _btSocket.ConnectAsync();
                                if (_btSocket.IsConnected)
                                {
                                    _readerBuffer = new InputStreamReader(_btSocket.InputStream);
                                    _writerBuffer = new OutputStreamWriter(_btSocket.OutputStream);
                                    System.Diagnostics.Debug.WriteLine("Connected!");
                                    _threadSleepTime = 10000;
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
                        }
                    }
                }
                if (_btSocket != null) _btSocket.Close();
                device = null;
                adapter = null;
                System.Diagnostics.Debug.WriteLine("Bluetooth connecting operation ended.");
            });
        }

        public void Stop()
        {
            if (_cancellationToken != null)
            {
                System.Diagnostics.Debug.WriteLine("Canceling bluetooth operation.");
                _cancellationToken.Cancel();
            }
        }

        public bool IsConnected()
        {
            //System.Diagnostics.Debug.WriteLine(_btSocket != null);
            //System.Diagnostics.Debug.WriteLine(_readerBuffer != null);
            //System.Diagnostics.Debug.WriteLine(_writerBuffer != null);
            //if (_btSocket != null) System.Diagnostics.Debug.WriteLine(_btSocket.IsConnected);
            return _btSocket != null && _readerBuffer != null && _writerBuffer != null ? _btSocket.IsConnected : false;
        }

        public void Write(string data)
        {
            if (!IsConnected())
            {
                System.Diagnostics.Debug.WriteLine("Cannot write to bluetooth device because it is not connected.");
                return;
            }
            try
            {
                _writerBuffer.Write(data, 0, data.Length);
                _writerBuffer.Flush();
                //_btSocket.OutputStream.WriteAsync(Encoding.ASCII.GetBytes(data), 0, data.Length);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
            }
        }

        public string Read()
        {
            string data = null;
            if (!IsConnected())
            {
                System.Diagnostics.Debug.WriteLine("Cannot read from bluetooth device because it is not connected.");
                return null;
            }
            try
            {
                BufferedReader buffer = new BufferedReader(_readerBuffer);
                if (buffer.Ready()) data = buffer.ReadLine();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + e.Message);
            }
            return data;
        }

        private BluetoothDevice FindDevice(BluetoothAdapter adapter, string btDevice)
        {
            foreach (var bluetoothDevice in adapter.BondedDevices)
            {
                if (bluetoothDevice.Name.ToUpper().Equals(btDevice.ToUpper()))
                {
                    System.Diagnostics.Debug.WriteLine("Found " + bluetoothDevice.Name + ". Try to connect with it...");
                    return bluetoothDevice;
                }
            }
            return null;
        }

        private BluetoothSocket CreateBluetoothSocket(BluetoothDevice device)
        {
            UUID uuid = UUID.FromString("00001101-0000-1000-8000-00805f9b34fb");
            if ((int)Android.OS.Build.VERSION.SdkInt >= 10) // Gingerbread 2.3.3 2.3.4
                return device.CreateInsecureRfcommSocketToServiceRecord(uuid);
            else
                return device.CreateRfcommSocketToServiceRecord(uuid);
        }
    }
}