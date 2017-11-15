using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using FriendOrganizer.UI.Wrapper;
using System.Net.Http.Formatting;

namespace FriendOrganizer.UI.Data.API.JokeAPI
{
    public class JokeService : IJokeService
    {
        static HttpClient httpClient = new HttpClient();

        public JokeService()
        {
            httpClient.BaseAddress = new Uri("https://08ad1pao69.execute-api.us-east-1.amazonaws.com/dev/");
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }


        public async Task<JokeWrapper> GetRandomJoke()
        {
            JokeWrapper item = null;
            HttpResponseMessage response = await httpClient.GetAsync("random_joke");

            if (response.IsSuccessStatusCode)
            {
                item = await response.Content.ReadAsAsync<JokeWrapper>();
            }
            return item;
        }
    }
}
