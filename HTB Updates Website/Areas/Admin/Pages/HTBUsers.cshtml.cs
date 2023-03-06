using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HTB_Updates_Website.Areas.Admin.Pages
{
    public class HTBUsersModel : PageModel
    {
        private readonly DatabaseContext _context;
        private readonly IAuthenticationManager _authenticationManager;

        public List<HTBUserInfo> HTBUsers { get; set; }

        public HTBUsersModel(DatabaseContext context, IAuthenticationManager authenticationManager)
        {
            _context = context;
            _authenticationManager = authenticationManager;
        }

        public class HTBUserInfo
        {
            public HTBUser HTBUser { get; set; }
            public int DiscordUsersCount { get; set; }
            public int SolvesCount { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authenticationManager.ValidateRequest(HttpContext)) return Redirect("/Authorize");
            HTBUsers = await _context.HTBUsers.Select(x => new HTBUserInfo { HTBUser = x, DiscordUsersCount = x.GuildUsers.Count, SolvesCount = x.Solves.Count }).ToListAsync();
            return Page();
        }
    }
}