using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    class EventVolunteerAssignmentModel : AbstractPestoModel
    {
        public string Name { get; set; }
        public ulong GuildId { get; set; }
        private ulong EventId { get; set; }
        private ulong UserId { get; set; }
    }
}
