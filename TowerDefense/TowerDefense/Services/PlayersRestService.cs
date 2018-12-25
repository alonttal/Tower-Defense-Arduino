using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace TowerDefense.Services
{
    class PlayersRestService
    {
        HttpClient client;
        public string ServiceName;

        public PlayersRestService(string serviceName)
        {
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 256000;
            ServiceName = serviceName;
        }

        public async Task<string> Login(string name, string password)
        {
            Uri uri = new Uri(ServiceName + "/api/Players/Login");
            string json = JsonConvert.SerializeObject(new { name, password });
            StringContent body = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(uri, body);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync();
            string token = JsonConvert.DeserializeObject<string>(content);
            System.Diagnostics.Debug.Print("Token is: " + token);
            return token;
        }

        public async Task<string> Register(string name, string password)
        {
            Uri uri = new Uri(ServiceName + "/api/Players");
            string json = JsonConvert.SerializeObject(new { name, password });
            StringContent body = new StringContent(json, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await client.PostAsync(uri, body);
            if (!response.IsSuccessStatusCode) return null;
            string content = await response.Content.ReadAsStringAsync();
            string token = JsonConvert.DeserializeObject<string>(content);
            System.Diagnostics.Debug.Print("Token is: " + token);
            return token;
        }
    }
}
