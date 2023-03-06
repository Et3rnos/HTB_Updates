using HTB_Updates_Shared_Resources;
using HTB_Updates_Shared_Resources.Managers;
using HTB_Updates_Shared_Resources.Models.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;
using System.Security.Claims;

namespace HTB_Updates_Website.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public ImagesController(DatabaseContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> OnGetSolveAsync()
        {
            var discordId = Convert.ToUInt64(HttpContext.User.FindFirstValue("DiscordId"));
            var discordAvatar = HttpContext.User.FindFirstValue("DiscordAvatar");
            var discordUsername = HttpContext.User.FindFirstValue("DiscordUsername");

            var discordUser = await _context.DiscordUsers
                .Include(x => x.GuildUsers).ThenInclude(x => x.HTBUser).ThenInclude(x => x.Solves.OrderBy(r => Guid.NewGuid()).Take(1))
                .Include(x => x.GuildUsers).ThenInclude(x => x.Guild)
                .FirstOrDefaultAsync(x => x.DiscordId == discordId);
            if (discordUser == null) return BadRequest();
            
            var guildUser = discordUser.GuildUsers.FirstOrDefault();
            if (guildUser == null) return BadRequest();

            var solve = guildUser.HTBUser.Solves.FirstOrDefault();
            if (solve == null) return BadRequest();

            var image = await ImageGeneration.GetSolvesImage(discordAvatar, discordUsername, null, guildUser, guildUser.HTBUser, solve);
            return File(image, "image/png", true);
        }
    }
}
