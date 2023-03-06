using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;
using System.Security.Cryptography;
using System.Text;

namespace HTB_Updates_Website.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _configuration;
        public string DiscordClientId { get; set; }
        public string DiscordClientSecret { get; set; }
        public string State { get; set; }

        public IndexModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnGet()
        {
            DiscordClientId = _configuration.GetValue<string>("DiscordClientId");
            DiscordClientSecret = _configuration.GetValue<string>("DiscordClientSecret");

            State = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            Response.Cookies.Append("State", State);
        }
    }
}