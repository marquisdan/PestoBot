using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.Guild
{
    class GuildSettingsModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Prefix { get; set; }
        public ulong ProjectReminderChannel { get; set; }
        public ulong TaskReminderChannel { get; set; }
        public ulong RunnerReminderChannel { get; set; }
        public ulong GuildId { get; set; }
    }
}
