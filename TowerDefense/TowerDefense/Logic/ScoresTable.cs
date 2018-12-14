using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Services;

namespace TowerDefense.Logic
{
    public class ScoresTable : INotifyPropertyChanged
    {
        private int _tableSize;
        private ScoresRestService _scoresRestService;
        private string _restService = Constants.Constants.RestApiService;
        private List<Score> _scores = new List<Score>();
        public List<Score> Scores
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

        public ScoresTable(int tableSize)
        {
            _tableSize = tableSize;
            _scoresRestService = new ScoresRestService(_restService);
            for (int i = 0; i < tableSize; i++) Scores.Add(new Score { Name = "Loading...", Value = 0 });
        }

        public void FetchScores()
        {
            Task<List<Score>> task = _scoresRestService.GetTopScores(_tableSize);
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

        public void PublishScore(Score score)
        {
            if (score.Value <= Scores.Last().Value)
            {
                return;
            }
            Score nextScore = score;
            for (int i = 0; i < _tableSize; i++)
            {
                if (score.Value > Scores[i].Value)
                {
                    Score tmpScore = new Score
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
            _scoresRestService.Post(score);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
