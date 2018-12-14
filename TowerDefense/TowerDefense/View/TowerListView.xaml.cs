using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TowerDefense
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class TowerListView : ContentView
	{
        public List<TowerView> towers = new List<TowerView>();

        public TowerListView(EventHandler TryToUpgradeTowerEventHandler)
        {
            InitializeComponent();

            StackLayout towersLayout = new StackLayout
            {
                Spacing = 10,
                Padding = 15,
                Orientation = StackOrientation.Vertical,
                VerticalOptions = LayoutOptions.CenterAndExpand
            };

            CreateTowers(towersLayout, TryToUpgradeTowerEventHandler);

            ScrollView towerListScrollView = new ScrollView() { Content = towersLayout };
            towerListScrollView.VerticalOptions = LayoutOptions.StartAndExpand;

            BindingContext = this;
            base.Content = towerListScrollView;
        }

        public void ResetTowers()
        {
            for (int i = 0; i < 6; i++)
            {
                towers[i].Tower.ResetTower();
            }
        }

        private void CreateTowers(StackLayout stackLayout, EventHandler TryToUpgradeTowerEventHandler)
        {
            for (int i = 0; i < 6; i++)
            {
                TowerView towerView = new TowerView();
                towerView.Tower.ID = i;
                towerView.Tower.Level = 1;
                towerView.Tower.NextLevelPrice = 5;
                towerView.Tower.Image = "tower" + (i + 1).ToString() + ".png";
                towerView.TryToUpgradeTowerEvent += TryToUpgradeTowerEventHandler;
                towers.Add(towerView);
                stackLayout.Children.Add(towerView);
            }
        }

    }
}