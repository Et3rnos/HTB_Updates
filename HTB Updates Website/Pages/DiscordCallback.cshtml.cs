using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace HTB_Updates_Website.Pages
{
    public class DiscordCallbackModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public DiscordCallbackModel(IConfiguration configuration)
        {
            _configuration= configuration;
        }

        public async Task<IActionResult> OnGetAsync(string code, string state)
        {
            if (Request.Cookies["State"] != state) return BadRequest();
            Response.Cookies.Delete("State");

            var clientId = _configuration.GetValue<string>("DiscordClientId");
            var clientSecret = _configuration.GetValue<string>("DiscordClientSecret");

            var client = new HttpClient();
            var values = new Dictionary<string, string>
            {
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "grant_type", "authorization_code" },
                { "code", code },
                { "scope", "identify" }
            };
            var content = new FormUrlEncodedContent(values);
            var response = await client.PostAsync("https://discordapp.com/api/oauth2/token", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var data = (dynamic)JsonConvert.DeserializeObject(responseString);

            if (string.IsNullOrEmpty((string)data.access_token)) return BadRequest();

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", (string)data.access_token);
            response = await client.GetAsync("https://discordapp.com/api/users/@me");
            responseString = await response.Content.ReadAsStringAsync();
            data = JsonConvert.DeserializeObject(responseString);

            if (string.IsNullOrEmpty((string)data.id) || string.IsNullOrEmpty((string)data.username)) return BadRequest();

            var discordId = (ulong)data.id;
            var discordUsername = (string)data.username;
            var discordAvatar = (string)data.avatar;
            var discordDiscriminator = (int)data.discriminator;

            discordAvatar = discordAvatar == null ? $"https://cdn.discordapp.com/embed/avatars/{discordDiscriminator % 5}.png" : $"https://cdn.discordapp.com/avatars/{discordId}/{discordAvatar}.png?size=256";

            var claims = new List<Claim>
            {
                new Claim("DiscordId", discordId.ToString()),
                new Claim("DiscordUsername", discordUsername),
                new Claim("DiscordAvatar", discordAvatar)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToPage("Settings");
        }
    }
}