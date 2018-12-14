using System;
using System.Collections.Generic;
using System.Text;

namespace TowerDefense
{
    public interface IBluetoothModule
    {
        void Start(string btDevice);
        void Stop();
        bool IsConnected();
        void Write(string data);
        string Read();
    }
}
