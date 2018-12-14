using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TowerDefense
{
    public class GameStats : INotifyPropertyChanged
    {
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
        private string _playerName = "Anonymous";
        public string PlayerName
        {
            get
            {
                return _playerName;
            }
            set
            {
                _playerName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PlayerName)));
            }
        }

        public void ResetStats()
        {
            Coins = 100;
            Score = 12;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
