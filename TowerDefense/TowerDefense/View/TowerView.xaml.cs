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
	public partial class TowerView : ContentView
	{
        public event EventHandler TryToUpgradeTowerEvent;
        public Tower Tower { get; set; }
        public bool ShowStats { get; set; } = false;

        public TowerView(Tower tower)
        {
            InitializeComponent();

            this.Tower = tower;
            InitStats();

            BindingContext = this;
        }

        private void InitStats()
        {
            for (int i = 0; i < Tower.Damage; i++)
            {
                Image starImage = new Image
                {
                    Source = "star.png"
                };
                damageLayout.Children.Add(starImage);
            }
            for (int i = 0; i < Tower.Speed; i++)
            {
                Image starImage = new Image
                {
                    Source = "star.png"
                };
                speedLayout.Children.Add(starImage);
            }
        }

        private void OnTapped(object sender, EventArgs e)
        {
            TryToUpgradeTowerEvent?.Invoke(this, null);
        }

        private void OnSwiped(object sender, EventArgs e)
        {
            ShowStats = !ShowStats;
            OnPropertyChanged(nameof(ShowStats));
        }
    }
}

