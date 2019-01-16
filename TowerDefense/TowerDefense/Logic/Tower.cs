using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TowerDefense
{
    public class Tower : INotifyPropertyChanged
    {
        public int ID = 0;
        private int _level = 0;
        public int Level
        {
            get
            {
                return _level;
            }
            set
            {
                _level = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Level)));
            }
        }
        private int _nextLevelPrice = 0;
        public int NextLevelPrice {
            get
            {
                return _nextLevelPrice;
            }
            set {
                _nextLevelPrice = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NextLevelPrice)));
            }
        }
        private string _image = "";
        public string Image
        {
            get
            {
                return _image;
            }
            set
            {
                _image = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Image)));

            }
        }
        public int Damage { get; set; } = 1;
        public int Speed { get; set; } = 1;

        public void LevelUp()
        {
            Level += 1;
            NextLevelPrice = NextLevelPrice + Level * 5; // TODO: change
        }

        public void ResetTower()
        {
            Level = 1;
            NextLevelPrice = 5;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
