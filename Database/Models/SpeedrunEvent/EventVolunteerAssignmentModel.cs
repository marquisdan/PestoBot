using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    class EventVolunteerAssignmentModel : AbstractPestoModel
    {
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ulong GuildId { get; set; }
        private ulong EventId { get; set; }
        private ulong UserId { get; set; }
    }
}
