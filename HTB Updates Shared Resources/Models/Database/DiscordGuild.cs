using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Shared_Resources.Models.Database
{
    public class DiscordGuild
    {
        [Key]
        public int Id { get; set; }
        public ulong GuildId { get; set; }
        public ulong ChannelId { get; set; }
        public bool OptionalAnnouncements { get; set; } = true;
        public bool MessageNewMembers { get; set; } = false;
        public List<GuildUser> GuildUsers { get; set; } = new List<GuildUser>();
    }
}
