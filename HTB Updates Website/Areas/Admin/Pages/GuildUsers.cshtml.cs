using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Models.Database;
using HTB_Updates_Website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace HTB_Updates_Website.Areas.Admin.Pages
{
    public class GuildUsersModel : PageModel
    {
        private readonly DatabaseContext _context;
        private readonly IAuthenticationManager _authenticationManager;

        public List<GuildUser> GuildUsers { get; set; }

        public GuildUsersModel(DatabaseContext context, IAuthenticationManager authenticationManager)
        {
            _context = context;
            _authenticationManager = authenticationManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!_authenticationManager.ValidateRequest(HttpContext)) return Redirect("/Authorize");
            GuildUsers = await _context.GuildUsers.ToListAsync();
            return Page();
        }
    }
}