using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Logic;
using TowerDefense.Managers;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;


namespace TowerDefense
{
    public partial class MainPage : ContentPage
    {
        BluetoothService BluetoothService = new BluetoothService("TowerDefense");
        GameStats GameStats = new GameStats();
        Player Player = new Player();
        PlayerManager PlayerManager;
        BluetoothGameManager BluetoothGameManager;

        GameStatsView gameStatsView;
        TowerListView towerListView;
        StatusView statusView;

        public MainPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);

            PlayerManager = new PlayerManager(Player);
            PlayerManager.InitPlayer();
            BluetoothGameManager = new BluetoothGameManager(BluetoothService, GameStats);

            gameStatsView = new GameStatsView(GameStats);
            towerListView = new TowerListView(TryToUpgradeTowerEventHandler);
            statusView = new StatusView(BluetoothService, GameStats, PlayerManager);

            statusView.StartGameEvent += StartGameEventHandler;
            statusView.EndGameEvent += EndGameEventHandler;
            BluetoothGameManager.EndGameEvent += EndGameEventHandler;

            BluetoothGameManager.StartTracking();

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
                GameStats.AtomicIncrementCoins(-tower.NextLevelPrice);
                tower.LevelUp();
                BluetoothService.Bluetooth.Write(tower.ID.ToString());
            }
            else System.Diagnostics.Debug.WriteLine("Not enough coins.");
        }

        public void StartGameEventHandler(object sender, EventArgs e)
        {
            if (!GameStats.IsGameStarted)
            {
                System.Diagnostics.Debug.WriteLine("Starting game.");
                GameStats.IsGameStarted = true;
                GameStats.ResetStats();
                towerListView.ResetTowers();
                BluetoothGameManager.SendStartGame();
            }
        }

        public void EndGameEventHandler(object sender, Boolean sendEndGame)
        {
            if (GameStats.IsGameStarted)
            {
                System.Diagnostics.Debug.WriteLine("Ending game.");
                GameStats.IsGameStarted = false;
                if (sendEndGame) BluetoothGameManager.SendEndGame();
                statusView.UpdateTableScore(new HighScore { Name = Player.Name, Value = GameStats.Score, Date = DateTime.Now });
                statusView.ExpandPanel();
            }
        }
    }
}
