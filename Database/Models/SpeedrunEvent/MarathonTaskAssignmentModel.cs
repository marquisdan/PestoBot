using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    public class MarathonTaskAssignmentModel : AbstractPestoModel
    {
        public DateTime TaskStartTime { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public ulong MarathonTaskId { get; set; }
    }
}
