using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Shared_Resources.Models.Database
{
    public class DiscordUser {
        [Key]
        public int Id { get; set; }
        public ulong DiscordId { get; set; }
        public int GuildId { get; set; }
        public DiscordGuild Guild { get; set; }
        public int HTBUserId { get; set; }
        public HTBUser HTBUser { get; set; }
        public bool Verified { get; set; } = false;
    }
}
