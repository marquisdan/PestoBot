using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpeedathonBot.Database.Models.Common;

namespace SpeedathonBot.Database.Models.Guild
{
    class GuildSettingsModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Prefix { get; set; }
        public ulong GuildId { get; set; }
    }
}
