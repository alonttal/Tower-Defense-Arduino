using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace TowerDefense.Logic
{
    public class Player : INotifyPropertyChanged
    {
        private string _name = Constants.Constants.DefaultPlayerName;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }
        private string _token;
        public string Token
        {
            get
            {
                return _token;
            }
            set
            {
                _token = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Token)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
