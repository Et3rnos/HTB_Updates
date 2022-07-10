using HTB_Updates_Discord_Bot.Models;
using HTB_Updates_Discord_Bot.Models.Api;
using HTB_Updates_Shared_Resources.Models.Shared;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot.Services
{
    public interface IHTBApiV4Service
    {
        Task<List<UnreleasedMachine>> GetUnreleasedMachines();
        Task<List<Solve>> GetSolves(int id);
        Task<string> GetUserNameById(int id);
    }

    public class HTBApiV4Service : IHTBApiV4Service
    {
        private static JwtSecurityToken token;
        private static DateTime tokenGenerationTime;
        private readonly IConfigurationRoot _configuration;

        public HTBApiV4Service(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetRequiredService<IConfigurationRoot>();
        }

        private async Task FillApiToken()
        {
            Log.Information("Generating a new API v4 token");

            if (DateTime.Now.Subtract(tokenGenerationTime).TotalMinutes < 30)
                throw new RateLimitingException("The bot attempted to login twice in just 30 minutes");
            tokenGenerationTime = DateTime.Now;

            var client = new HttpClient();
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string> {
                    { "email", _configuration.GetValue<string>("HTBUsername") },
                    { "password", _configuration.GetValue<string>("HTBPassword") },
                    { "remember", "true" }
                }
            );
            var response = await client.PostAsync("https://www.hackthebox.com/api/v4/login", content);
            dynamic json = JsonConvert.DeserializeObject(await response.Content.ReadAsStringAsync());
            token = new JwtSecurityToken((string)json.message?.access_token);
        }

        public async Task<List<UnreleasedMachine>> GetUnreleasedMachines()
        {
            var response = await MakeApiCall("https://www.hackthebox.com/api/v4/machine/unreleased");
            dynamic json = JsonConvert.DeserializeObject(response);
            var jArray = (JArray)json.data;
            var unreleasedMachines = JsonConvert.DeserializeObject<List<UnreleasedMachine>>(jArray.ToString());
            return unreleasedMachines;
        }

        public async Task<List<Solve>> GetSolves(int id)
        {
            var response = await MakeApiCall($"https://www.hackthebox.com/api/v4/user/profile/activity/{id}");
            dynamic json = JsonConvert.DeserializeObject(response);
            var jArray = (JArray)json.profile.activity;
            var solves = JsonConvert.DeserializeObject<List<Solve>>(jArray.ToString());
            return solves;
        }

        public async Task<string> GetUserNameById(int id)
        {
            var response = await MakeApiCall($"https://www.hackthebox.com/api/v4/user/profile/basic/{id}");
            dynamic json = JsonConvert.DeserializeObject(response);
            return (string)json.profile.name;
        }

        private async Task<string> MakeApiCall(string url)
        {
            var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
            
            if (token != null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.RawData);
            var response = await client.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Redirect)
            {
                await FillApiToken();
                return await MakeApiCall(url);
            }

            return await response.Content.ReadAsStringAsync();
        }
    }
}
