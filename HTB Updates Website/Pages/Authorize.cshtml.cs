using HTB_Updates_Website.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;

namespace HTB_Updates_Website.Pages
{
    public class AuthorizeModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthenticationManager _authenticationManager;

        public AuthorizeModel(IConfiguration configuration, IAuthenticationManager authenticationManager)
        {
            _configuration = configuration;
            _authenticationManager = authenticationManager;
        }

        public IActionResult OnGet()
        {
            if (_authenticationManager.ValidateRequest(HttpContext)) 
                return Redirect("/Admin/Guilds");
            return Page();
        }

        public IActionResult OnPost(string password)
        {
            if (_authenticationManager.ValidateRequest(HttpContext)) return Redirect("/Admin/Guilds");

            var token = _authenticationManager.GetToken(password);
            if (token == null) return Page();

            HttpContext.Response.Cookies.Append("token", token);
            return Redirect("/Admin/Guilds");
        }
    }
}