using HTB_Updates_Discord_Bot.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot.Services
{
    public interface IHTBApiV1Service
    {
        Task<int> GetHTBUserIdByName(string username);
        Task<int> GetHTBIdByAccountId(string accountId);
    }

    public class HTBApiV1Service : IHTBApiV1Service
    {
        private readonly HttpClient _client;
        private readonly IConfigurationRoot _configuration;

        public HTBApiV1Service(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
            _client = new HttpClient { BaseAddress = new Uri("https://www.hackthebox.com") };
        }

        public async Task<int> GetHTBUserIdByName(string username)
        {
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string> {
                    { "username", username },
                }
            );
            var response = await _client.PostAsync(AddTokenToQuery("/api/user/id"), content);
            dynamic user = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            if(user == null)
            {
                return -1;
            }
            return (int)user.id;
        }

        public async Task<int> GetHTBIdByAccountId(string accountId) {
            var response = await _client.GetAsync(AddTokenToQuery($"/api/users/identifier/{accountId}"));
            if (!response.IsSuccessStatusCode) {
                return -1;
            }
            dynamic user = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            return (int)user.user_id;
        }

        private string AddTokenToQuery(string query)
        {
            return query + "?api_token=" + _configuration.GetValue<string>("V1ApiToken");
        }
    }
}