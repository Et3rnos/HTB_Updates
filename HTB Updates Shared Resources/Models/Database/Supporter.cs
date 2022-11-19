using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Shared_Resources.Models.Database
{
    public class Supporter {
        [Key]
        public Guid Id { get; set; }
        public ulong DiscordId { get; set; }
        public string Slogan { get; set; }
        public bool Border { get; set; }
    }
}
