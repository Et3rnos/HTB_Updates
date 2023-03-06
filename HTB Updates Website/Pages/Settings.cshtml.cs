using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Models.Database;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HTB_Updates_Website.Pages
{
    [Authorize]
    public class SettingsModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _context;

        public DiscordUser DiscordUser { get; set; }
        public string DiscordUsername { get; set; }

        public SettingsModel(IConfiguration configuration, DatabaseContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public async Task OnGetAsync()
        {
            var discordId = Convert.ToUInt64(HttpContext.User.FindFirstValue("DiscordId"));
            DiscordUsername = HttpContext.User.FindFirstValue("DiscordUsername");
            DiscordUser = await _context.DiscordUsers
                .Include(x => x.GuildUsers).ThenInclude(x => x.HTBUser)
                .Include(x => x.GuildUsers).ThenInclude(x => x.Guild)
                .FirstOrDefaultAsync(x => x.DiscordId == discordId);
        }
    }
}