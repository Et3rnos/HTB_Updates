using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot.Models.Database
{
    public class DiscordGuild
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public List<DiscordUser> DiscordUsers { get; set; } = new List<DiscordUser>();
    }
}
