using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Services;

namespace TowerDefense.Logic
{
    public class PlayerManager : INotifyPropertyChanged
    {
        public Player Player { get; set; }
        private string _restService = Constants.Constants.RestApiService;
        private bool _isConnected = false;
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
            set
            {
                _isConnected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsConnected)));
            }
        }

        public PlayerManager(Player player)
        {
            this.Player = player;
        }

        public void InitPlayer()
        {
            if (App.Current.Properties.ContainsKey("PlayerName") && App.Current.Properties.ContainsKey("PlayerToken") && !App.Current.Properties["PlayerName"].Equals(Constants.Constants.DefaultPlayerName))
            {
                Player.Name = App.Current.Properties["PlayerName"] as string;
                Player.Token = App.Current.Properties["PlayerToken"] as string;
                System.Diagnostics.Debug.Print("Player is not null. Name: " + Player.Name + " Token: " + Player.Token);
                IsConnected = true;
            }
        }

        public async Task<bool> Login(string name, string password)
        {
            if (name == null || password == null || name.Length > 10 || name.Length < 1 || password.Length > 128 || password.Length < 6) return false;
            string token = await new PlayersRestService(_restService).Login(name, password);
            if (token == null) return false;
            UpdatePlayer(name, token);
            return true;

        }

        public async Task<bool> Register(string name, string password)
        {
            if (name == null || password == null || name.Length > 10 || name.Length < 1 || password.Length > 128 || password.Length < 6) return false;
            string token = await new PlayersRestService(_restService).Register(name, password);
            if (token == null) return false;
            UpdatePlayer(name, token);
            return true;
        }

        public void Disconnect()
        {
            UpdatePlayer(Constants.Constants.DefaultPlayerName, "");
            IsConnected = false;
        }

        private void UpdatePlayer(string name, string token)
        {
            Player.Name = name;
            Player.Token = token;
            App.Current.Properties["PlayerName"] = name;
            App.Current.Properties["PlayerToken"] = token;
            IsConnected = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
