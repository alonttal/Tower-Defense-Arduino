using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Logic;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TowerDefense
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class HighScoresView : ContentView
    {

        public int ScoresTableSize = Constants.Constants.ScoresTableSize;
        public HighScoresManager ScoresTable { get; set; }

        public HighScoresView()
        {
            InitializeComponent();
            ScoresTable = new HighScoresManager(ScoresTableSize);

            StackLayout HighscoresLayout = new StackLayout
            {
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            HighscoresLayout.Children.Add(new Label { Text = "Hall of Fame:", FontAttributes = FontAttributes.Bold, FontSize = 15 });

            StackLayout ScoresTableLayout = new StackLayout();
            ScoresTableLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
            int scoreNumber = 1;
            foreach (HighScore score in ScoresTable.Scores)
            {
                Image crownImage = new Image();
                crownImage.HorizontalOptions = LayoutOptions.CenterAndExpand;
                crownImage.VerticalOptions = LayoutOptions.CenterAndExpand;
                if (scoreNumber <= 3)
                {
                    crownImage.Source = "crown" + scoreNumber + ".png";
                    scoreNumber++;
                }

                Label nameLabel = new Label();
                Label valueLabel = new Label();
                nameLabel.BindingContext = score;
                valueLabel.BindingContext = score;
                nameLabel.SetBinding(Label.TextProperty, "Name");
                valueLabel.SetBinding(Label.TextProperty, "Value");
                nameLabel.HorizontalTextAlignment = TextAlignment.Start;
                valueLabel.HorizontalTextAlignment = TextAlignment.End;

                StackLayout imageLayout = new StackLayout();
                imageLayout.WidthRequest = 30;
                imageLayout.HeightRequest = 30;
                imageLayout.HorizontalOptions = LayoutOptions.CenterAndExpand;
                imageLayout.VerticalOptions = LayoutOptions.CenterAndExpand;
                StackLayout nameLayout = new StackLayout();
                nameLayout.HorizontalOptions = LayoutOptions.StartAndExpand;
                nameLayout.Padding = new Thickness(0, 0, 80, 0);
                nameLayout.HorizontalOptions = LayoutOptions.StartAndExpand;
                nameLayout.VerticalOptions = LayoutOptions.CenterAndExpand;
                StackLayout valueLayout = new StackLayout();
                valueLayout.HorizontalOptions = LayoutOptions.EndAndExpand;
                valueLayout.VerticalOptions = LayoutOptions.CenterAndExpand;
                imageLayout.Children.Add(crownImage);
                nameLayout.Children.Add(nameLabel);
                valueLayout.Children.Add(valueLabel);
                StackLayout scoreLayout = new StackLayout();
                scoreLayout.Orientation = StackOrientation.Horizontal;
                scoreLayout.Children.Add(imageLayout);
                scoreLayout.Children.Add(nameLayout);
                scoreLayout.Children.Add(valueLayout);
                ScoresTableLayout.Children.Add(scoreLayout);
            }

            HighscoresLayout.Children.Add(ScoresTableLayout);
            Content = HighscoresLayout;
            LoadScores();
        }

        private void LoadScores()
        {
            Task.Run(() =>
            {
                ScoresTable.FetchScores();
                System.Diagnostics.Debug.Print("Done loading scores");
            });
        }
    }
}