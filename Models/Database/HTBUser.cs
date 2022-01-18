using HTB_Updates_Discord_Bot.Models.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot.Models.Database
{
    public class HTBUser {
        [Key]
        public int Id { get; set; }
        public int HtbId { get; set; }
        public string Username { get; set; }
        public int Score { get; set; }
        public List<Solve> Solves { get; set; } = new List<Solve>();
        public List<DiscordUser> DiscordUsers { get; set; } = new List<DiscordUser>();
        public DateTime LastUpdated {get; set;}
    }
}
