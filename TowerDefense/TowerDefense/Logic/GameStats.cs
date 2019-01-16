using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TowerDefense
{
    public class GameStats : INotifyPropertyChanged
    {
        // operations on 32-bit values are atomic in C#
        private readonly Object _lockObj = new Object();
        private int _coins = 0;
        public int Coins {
            get {
                return _coins;
            }
            set
            {
                _coins = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Coins)));
            }
        }
        private int _score = 0;
        public int Score
        {
            get
            {
                return _score;
            }
            set
            {
                _score = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Score)));
            }
        }
        private bool _isGameStarted = false;
        public bool IsGameStarted
        {
            get
            {
                return _isGameStarted;
            }
            set
            {
                _isGameStarted = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsGameStarted)));
            }
        }

        public void ResetStats()
        {
            Coins = 0;
            Score = 0;
        }

        public void AtomicIncrementCoins(int value)
        {
            lock (_lockObj)
            {
                Coins += value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
