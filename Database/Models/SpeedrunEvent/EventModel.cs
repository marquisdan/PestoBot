using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    public class EventModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Name { get; set; }
        public string ScheduleLink { get; set; }
        public ulong GuildId { get; set; }
    }
}