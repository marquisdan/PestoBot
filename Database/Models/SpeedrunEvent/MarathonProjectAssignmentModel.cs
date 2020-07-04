using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    public class MarathonProjectAssignmentModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int MarathonProjectId { get; set; }
    }
}
