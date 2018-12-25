using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TowerDefense.Logic;

namespace TowerDefense.Services
{
    class HighScoresRestService
    {
        HttpClient client;
        public string ServiceName;

        public HighScoresRestService(string serviceName)
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            ServiceName = serviceName;
        }

        public async Task<List<HighScore>> GetTopScores(int number)
        {
            if (number < 0) return null;
            Uri uri = new Uri(ServiceName + "/api/Scores/Top/" + number.ToString());
            System.Diagnostics.Debug.Print("Requesting scores from URI: " + uri);
            HttpResponseMessage response = await client.GetAsync(uri);
            List<HighScore> scores;
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.Print("Request content is: " + content);
                scores = JsonConvert.DeserializeObject<List<HighScore>>(content);
            }
            else
            {
                scores = new List<HighScore>();
            }
            if (scores.Count < number)
            {
                for (int i = 0; i < number - scores.Count; i++)
                    scores.Add(new HighScore { Name = "Anonymous", Value = 0, Date = DateTime.MinValue });
            }
            return scores;
        }

        public async void Post(HighScore score, string token)
        {
            System.Diagnostics.Debug.Print("Uploading score to cloud: " + (score != null? score.Name + " " + score.Value : ""));
            if (score == null || score.Value < 0 || score.Name == null || score.Name.Length > 10) return;
            string json = JsonConvert.SerializeObject(score);
            HttpRequestMessage request = new HttpRequestMessage()
            {
                RequestUri = new Uri(ServiceName + "/api/Scores"),
                Method = HttpMethod.Post,
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            System.Diagnostics.Debug.Print("token: " + token);
            request.Headers.Add(Constants.Constants.AuthHeader, token);
            HttpResponseMessage response = await client.SendAsync(request);
        }
    }
}
