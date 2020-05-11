using System;
using SpeedathonBot.Database.Models.Common;

namespace SpeedathonBot.Database.Models.SpeedrunEvent
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
