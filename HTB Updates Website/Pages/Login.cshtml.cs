using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace HTB_Updates_Website.Pages
{
    public class LoginModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public LoginModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IActionResult OnGet()
        {
            var discordClientId = _configuration.GetValue<string>("DiscordClientId");

            var state = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            Response.Cookies.Append("State", state);

            return Redirect($"https://discord.com/oauth2/authorize?response_type=code&client_id={discordClientId}&response_type=code&scope=identify&state={state}");
        }
    }
}