using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    public class MarathonProjectAssignmentModel : AbstractPestoModel
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public int MarathonProjectId { get; set; }
    }
}
