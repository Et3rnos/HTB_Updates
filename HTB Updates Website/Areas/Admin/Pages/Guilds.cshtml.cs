using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HTB_Updates_Website.Areas.Admin.Pages
{
    public class GuildsModel : PageModel
    {
        private readonly DatabaseContext _context;
        private readonly IAuthenticationManager _authenticationManager;

        public List<DiscordGuildInfo> DiscordGuilds { get; set; }

        public GuildsModel(DatabaseContext context, IAuthenticationManager authenticationManager)
        {
            _context = context;
            _authenticationManager = authenticationManager;
        }

        public class DiscordGuildInfo
        {
            public DiscordGuild DiscordGuild { get; set; }
            public int LinkedUsersCount { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authenticationManager.ValidateRequest(HttpContext)) return Redirect("/Authorize");
            DiscordGuilds = await _context.DiscordGuilds.Select(x => new DiscordGuildInfo { DiscordGuild = x, LinkedUsersCount = x.GuildUsers.Count }).ToListAsync();
            return Page();
        }
    }
}