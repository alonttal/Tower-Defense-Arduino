﻿
using System;
using TowerDefense.Logic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TowerDefense
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class StatusView : ContentView
    {
        public event EventHandler StartGameEvent;
        public event EventHandler EndGameEvent;

        public bool IsExpanded { get; set; } = true;

        public GameStats GameStats { get; set; }
        public BluetoothService Bluetooth { get; set; }

        public StatusView(BluetoothService bluetooth, GameStats gameStats)
        {
            InitializeComponent();

            this.GameStats = gameStats;
            this.Bluetooth = bluetooth;

            Bluetooth.TrackBluetoothConnectionStatus();

            BindingContext = this;
        }

        public void UpdateTableScore(Score score)
        {
            if (score.Value > 0)
                HighScores.ScoresTable.PublishScore(score);
        }

        private void MovePanel()
        {
            double currentHeight = Height;
            if (!IsExpanded)
            {
                Animation animate = new Animation(d => HeightRequest = d, currentHeight, currentHeight + ExpandedLayout.Height, Easing.SinIn);
                animate.Commit(this, "ExpandPanel", 20, 500);
            }
            else
            {
                Animation animate = new Animation(d => HeightRequest = d, currentHeight, currentHeight - ExpandedLayout.Height, Easing.SinOut);
                animate.Commit(this, "CollapsePanel", 20, 500);
            }
            IsExpanded = !IsExpanded;
            OnPropertyChanged(nameof(IsExpanded));
        }

        void MovePanel_Tapped(object sender, System.EventArgs e)
        {
            MovePanel();
        }

        private void GameStart_Clicked(object sender, System.EventArgs e)
        {
            StartGameEvent?.Invoke(this, null);
            MovePanel();
        }

        private void GameEnd_Clicked(object sender, EventArgs e)
        {
            EndGameEvent?.Invoke(this, null);
        }
    }
}