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
	public partial class GameStatsView : ContentView
	{
        public GameStats GameStats { get; set; }

		public GameStatsView (GameStats gameStats)
		{
			InitializeComponent ();
            this.GameStats = gameStats;

            BindingContext = this;
		}
	}
}