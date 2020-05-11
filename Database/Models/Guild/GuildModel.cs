using System;
using SpeedathonBot.Database.Models.Common;

namespace SpeedathonBot.Database.Models.Guild
{
    class GuildModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime JoinDate { get; set; }
        public string Name { get; set; }
        public string OwnerUsername { get; set; }
        public ulong OwnerId { get; set; }
        public DateTime LastConnection { get; set; }
    }
}
