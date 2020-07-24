using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.SpeedrunEvent
{
    public class ReminderModel : AbstractPestoModel
    {
        public DateTime LastSent { get; set; }
        public string Content { get; set; }
        public int Interval { get; set; }
        public int Type { get; set; }
        public ulong AssignmentId { get; set; }
        public ulong UserId { get; set; }
        public ulong GuildId { get; set; }
    }
}