using System;

namespace PestoBot.Database.Models.Common
{
    public class GlobalSettingsModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public bool DebugRemindersEnabled { get; set; }
        public int DebugReminderHour { get; set; }
        public int DebugReminderMinutes { get; set; }
    }
}