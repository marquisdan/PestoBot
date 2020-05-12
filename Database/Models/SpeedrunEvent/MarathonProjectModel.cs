using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    class MarathonProjectModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ulong GuildId { get; set; }
        public ulong EventAssignmentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string ReminderText { get; set; }
        public DateTime LastReminderSent { get; set; }
    }
}
