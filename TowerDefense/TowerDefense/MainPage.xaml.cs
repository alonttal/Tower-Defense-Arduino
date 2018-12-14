using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace TowerDefense
{
    public partial class MainPage : ContentPage
    {
        BluetoothService BluetoothService = new BluetoothService("TowerDefense");
        GameStats GameStats = new GameStats();

        GameStatsView gameStatsView;
        TowerListView towerListView;
        StatusView statusView;

        public MainPage()
        {
            InitializeComponent();
            gameStatsView = new GameStatsView(GameStats);
            towerListView = new TowerListView(TryToUpgradeTowerEventHandler);
            statusView = new StatusView(BluetoothService, GameStats);

            statusView.StartGameEvent += StartGameEventHandler;
            statusView.EndGameEvent += EndGameEventHandler;

            StackLayout mainLayout = new StackLayout();
            mainLayout.Spacing = 0;
            mainLayout.Children.Add(gameStatsView);
            mainLayout.Children.Add(towerListView);
            mainLayout.Children.Add(statusView);

            Content = mainLayout;
        }

        public void TryToUpgradeTowerEventHandler(object sender, EventArgs e) {
            Tower tower = ((TowerView)sender).Tower;
            if (GameStats.Coins >= tower.NextLevelPrice)
            {
                GameStats.Coins -= tower.NextLevelPrice;
                tower.LevelUp();
                BluetoothService.Bluetooth.Write(tower.ID.ToString());
            }
            else System.Diagnostics.Debug.WriteLine("Not enough coins.");
        }

        public void StartGameEventHandler(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Starting game.");
            GameStats.ResetStats();
            towerListView.ResetTowers();
            BluetoothService.Bluetooth.Write("s");
            GameStats.IsGameStarted = true;
        }

        public void EndGameEventHandler(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Ending game.");
            BluetoothService.Bluetooth.Write("e");
            statusView.UpdateTableScore(new Logic.Score { Name = GameStats.PlayerName, Value = GameStats.Score, Date = DateTime.Now });
            GameStats.IsGameStarted = false;
        }


    }
}
