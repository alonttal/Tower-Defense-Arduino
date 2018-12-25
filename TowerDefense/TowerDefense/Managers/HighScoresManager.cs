using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Services;

namespace TowerDefense.Logic
{
    public class HighScoresManager : INotifyPropertyChanged
    {
        private int _tableSize;
        private HighScoresRestService _scoresRestService;
        private string _restService = Constants.Constants.RestApiService;
        private List<HighScore> _scores = new List<HighScore>();
        public List<HighScore> Scores
        {
            get
            {
                return _scores;
            }
            private set
            {
                _scores = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Scores)));
            }
        }

        public HighScoresManager(int tableSize)
        {
            _tableSize = tableSize;
            _scoresRestService = new HighScoresRestService(_restService);
            for (int i = 0; i < tableSize; i++) Scores.Add(new HighScore { Name = "Loading...", Value = 0 });
        }

        public void FetchScores()
        {
            Task<List<HighScore>> task = _scoresRestService.GetTopScores(_tableSize);
            task.Wait();
            for (int i = 0; i < _tableSize; i++)
            {
                Scores[i].Name = task.Result[i].Name;
                System.Diagnostics.Debug.Print("Name: " + Scores[i].Name);
                Scores[i].Value = task.Result[i].Value;
                System.Diagnostics.Debug.Print("Value: " + task.Result[i].Value);

                Scores[i].Date = task.Result[i].Date;
            }
        }

        public void PublishScore(HighScore score, string token)
        {
            _scoresRestService.Post(score, token);
            if (score.Value <= Scores.Last().Value)
            {
                return;
            }
            HighScore nextScore = score;
            for (int i = 0; i < _tableSize; i++)
            {
                if (score.Value > Scores[i].Value)
                {
                    HighScore tmpScore = new HighScore
                    {
                        Name = Scores[i].Name,
                        Value = Scores[i].Value,
                        Date = Scores[i].Date
                    };
                    Scores[i].Name = nextScore.Name;
                    Scores[i].Value = nextScore.Value;
                    Scores[i].Date = nextScore.Date;
                    nextScore = tmpScore;
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
