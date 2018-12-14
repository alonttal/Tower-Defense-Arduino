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
        public Tower Tower { get; set; } = new Tower();

        public TowerView()
        {
            InitializeComponent();
            BindingContext = this;
        }

        private void OnTapped(object sender, EventArgs e)
        {
            TryToUpgradeTowerEvent?.Invoke(this, null);
        }
    }
}

