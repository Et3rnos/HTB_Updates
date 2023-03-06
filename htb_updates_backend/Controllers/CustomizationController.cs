using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using HTB_Updates_Shared_Resources;
using Microsoft.EntityFrameworkCore;
using HTB_Updates_Shared_Resources.Models.Database;

namespace htb_updates_backend.Controllers
{
    [ApiController]
    [Route("/api/customization")]
    public class CustomizationController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly DatabaseContext _context;

        public CustomizationController(IConfiguration configuration, DatabaseContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        [HttpGet]
        [Route("get")]
        [Authorize]
        public async Task<IActionResult> GetCustomizationAsync()
        {
            var discordId = Convert.ToUInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var username = User.FindFirstValue(ClaimTypes.Name);
            var discriminator = User.FindFirstValue("Discriminator");

            var discordUser = await _context.DiscordUsers.FirstOrDefaultAsync(x => x.DiscordId == discordId);
            if (discordUser == null)
            {
                discordUser = new DiscordUser
                {
                    DiscordId = discordId,
                    Border = false
                };
                await _context.DiscordUsers.AddAsync(discordUser);
                await _context.SaveChangesAsync();
            }
                
            return Ok(new
            {
                discordUser.Border
            });
        }

        public class CustomizationModel
        {
            [Required]
            public bool Border { get; set; }
        }

        [HttpPost]
        [Route("update")]
        [Authorize]
        public async Task<IActionResult> UpdateCustomizationAsync([FromBody] CustomizationModel model)
        {
            var discordId = Convert.ToUInt64(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var username = User.FindFirstValue(ClaimTypes.Name);
            var discriminator = User.FindFirstValue("Discriminator");

            var discordUser = await _context.DiscordUsers.FirstOrDefaultAsync(x => x.DiscordId == discordId);
            if (discordUser == null)
            {
                discordUser = new DiscordUser
                {
                    DiscordId = discordId,
                    Border = model.Border
                };
                await _context.DiscordUsers.AddAsync(discordUser);
            }
            else
            {
                discordUser.Border = model.Border;
            }

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
