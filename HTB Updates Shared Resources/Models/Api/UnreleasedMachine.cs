using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Shared_Resources.Models.Api
{
    public class UnreleasedMachine
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Os { get; set; }
        public string Avatar { get; set; }
        public DateTime Release { get; set; }
        public int Difficulty { get; set; }
        [JsonProperty("difficulty_text")]
        public string DifficultyText { get; set; }
    }
}
