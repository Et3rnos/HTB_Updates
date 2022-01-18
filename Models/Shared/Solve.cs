using HTB_Updates_Discord_Bot.Models.Database;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HTB_Updates_Discord_Bot.Models.Shared
{
    public class Solve
    {
        [JsonIgnore]
        public int Id { get; set; }

        public DateTime Date { get; set; }

        [JsonProperty("date_diff")]
        public string DateDiff { get; set; }

        [JsonProperty("object_type")]
        public string ObjectType { get; set; }

        public string Type { get; set; }

        [JsonProperty("first_blood")]
        public bool FirstBlood { get; set; }

        [JsonProperty("id")]
        public int SolveId { get; set; }

        public string Name { get; set; }

        public int Points { get; set; }

        [JsonProperty("machine_avatar")]
        public string MachineAvatar { get; set; }

        [JsonProperty("challenge_category")]
        public string ChallengeCategory { get; set; }

        [JsonIgnore]
        public int HTBUserId { get; set; }

        [JsonIgnore]
        public HTBUser HTBUser { get; set; }
    }

    public sealed class SolveComparer : IEqualityComparer<Solve> {
        public bool Equals(Solve x, Solve y) {
            if (x == null)
                return y == null;
            else if (y == null)
                return false;
            else
                return x.Type == y.Type && x.SolveId == y.SolveId;
        }

        public int GetHashCode(Solve obj) {
            return HashCode.Combine(obj.Type, obj.SolveId);
        }
    }
}