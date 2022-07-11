using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HTB_Updates_Website.Areas.Admin.Pages
{
    public class DiscordUsersModel : PageModel
    {
        private readonly DatabaseContext _context;
        private readonly IAuthenticationManager _authenticationManager;

        public List<DiscordUserInfo> DiscordUsers { get; set; }

        public DiscordUsersModel(DatabaseContext context, IAuthenticationManager authenticationManager)
        {
            _context = context;
            _authenticationManager = authenticationManager;
        }

        public class DiscordUserInfo
        {
            public DiscordUser DiscordUser { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authenticationManager.ValidateRequest(HttpContext)) return Redirect("/Authorize");
            DiscordUsers = await _context.DiscordUsers.Select(x => new DiscordUserInfo { DiscordUser = x }).ToListAsync();
            return Page();
        }
    }
}