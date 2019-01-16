using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Logic;
using TowerDefense.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TowerDefense
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SettingsPage : ContentPage
	{
        public PlayerManager PlayerManager { get; set; }
        public bool IsLoggingIn { get; set; } = false;
        public bool IsRegistering { get; set; } = false;

        public SettingsPage (PlayerManager playerManager)
		{
			InitializeComponent ();
            this.PlayerManager = playerManager;

            Title = "Settings";
            BindingContext = this;
		}

        private async void OnConnect_Clicked(object sender, EventArgs e)
        {
            IsLoggingIn = true;
            OnPropertyChanged("IsLoggingIn");
            string name = LoginNameEntry.Text;
            string password = LoginPasswordEntry.Text;
            if (await CheckProperties(name, password, "Login Failed"))
            {
                bool success = await PlayerManager.Login(name, password);
                if (success) await DisplayAlert("Success", "Commander " + name + ",\nYou have connected successfully", "OK");
                else await DisplayAlert("Login Failed", name + " is not a commander I am aware of!\nAre you trying to fool me?", "OK");
            }
            IsLoggingIn = false;
            OnPropertyChanged("IsLoggingIn");
        }

        private async void OnRegister_Clicked(object sender, EventArgs e)
        {
            IsRegistering = true;
            OnPropertyChanged("IsRegistering");
            string name = RegisterNameEntry.Text;
            string password = RegisterPasswordEntry.Text;
            if (await CheckProperties(name, password, "Registeration Failed"))
            {
                bool success = await PlayerManager.Register(name, password);
                if (success) await DisplayAlert("Success", "Thank you commander " + name + ",\nYou are now Registered", "Thanks!");
                else await DisplayAlert("Name Already Exists", name + ", I don't like your name and password, you better try harder", "OK");
            }
            IsRegistering = false;
            OnPropertyChanged("IsRegistering");
        }
        
        private void OnDisconnect_Clicked(object sender, EventArgs e)
        {
            PlayerManager.Disconnect();
        }

        private async Task<bool> CheckProperties(string name, string password, string title)
        {
            if (name == null || name.Length > 10 || name.Length < 1)
            {
                await DisplayAlert(title, "Argh... This is not a valid name!", "OK");
                return false;
            }
            if (password == null || password.Length > 128 || password.Length < 6)
            {
                await DisplayAlert(title, "what is that password?!", "OK");
                return false;
            }
            return true;
        }
    }
}