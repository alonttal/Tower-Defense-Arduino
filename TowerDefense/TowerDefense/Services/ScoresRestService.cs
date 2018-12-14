using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Logic;

namespace TowerDefense.Services
{
    class ScoresRestService
    {
        HttpClient client;
        public string ServiceName;

        public ScoresRestService(string serviceName)
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            ServiceName = serviceName;
        }

        public async Task<List<Score>> GetTopScores(int number)
        {
            if (number < 0) return null;
            Uri uri = new Uri(ServiceName + "/api/Scores/Top/" + number.ToString());
            System.Diagnostics.Debug.Print("Requesting scores from URI: " + uri);
            HttpResponseMessage response = await client.GetAsync(uri);
            List<Score> scores;
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.Print("Request content is: " + content);
                scores = JsonConvert.DeserializeObject<List<Score>>(content);
            }
            else
            {
                scores = new List<Score>();
            }
            if (scores.Count < number)
            {
                for (int i = 0; i < number - scores.Count; i++)
                    scores.Add(new Score { Name = "Anonymous", Value = 0, Date = DateTime.MinValue });
            }
            return scores;
        }

        public async void Post(Score score)
        {
            if (score == null || score.Value < 0 || score.Name == null || score.Name.Length > 10) return;
            Uri uri = new Uri(ServiceName + "/api/Scores");
            string json = JsonConvert.SerializeObject(score);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(uri, content);
        }
    }
}
