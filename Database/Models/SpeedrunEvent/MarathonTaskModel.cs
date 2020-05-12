using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    public class MarathonTaskModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ulong GuildId { get; set; }
        public ulong EventId { get; set; }
    }
}
