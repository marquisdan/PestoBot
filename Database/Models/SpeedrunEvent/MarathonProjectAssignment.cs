using System;
using SpeedathonBot.Database.Models.Common;

namespace SpeedathonBot.Database.Models.SpeedrunEvent
{
    public class MarathonProjectAssignment : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int MarathonProjectId { get; set; }
    }
}
