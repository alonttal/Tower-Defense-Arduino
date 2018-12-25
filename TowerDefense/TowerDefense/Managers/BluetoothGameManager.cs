using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TowerDefense.Managers
{
    class BluetoothGameManager
    {
        private BluetoothService _bluetoothService;
        private GameStats _gameStats;
        private bool _isTracking = false;
        private Task _task;

        public BluetoothGameManager(BluetoothService bluetoothService, GameStats gameStats)
        {
            this._bluetoothService = bluetoothService;
            this._gameStats = gameStats;
        }

        private void StartTracking()
        {
            _isTracking = true;
            _task = Task.Run(() =>
            {
                while (_isTracking)
                {
                    string strMessage = _bluetoothService.Bluetooth.Read();
                    if (strMessage != null && int.TryParse(strMessage, out int deltaScore))
                    {
                        _gameStats.Score += deltaScore;

                        _gameStats.AtomicIncrementCoins(deltaScore);
                    }
                    else if (strMessage != null && strMessage.Equals("e"))
                    {
                        StopTracking();
                    }
                }
                System.Diagnostics.Debug.Print("Score Tracking Ended");
            });
        }

        private void StopTracking()
        {
            _isTracking = false;
        }

        public void SendStartGame()
        {
            StartTracking();
            _bluetoothService.Bluetooth.Write("s");
        }
        public void SendEndGame()
        {
            StopTracking();
            _bluetoothService.Bluetooth.Write("e");
            _task.Wait(5000);
        }
    }
}
