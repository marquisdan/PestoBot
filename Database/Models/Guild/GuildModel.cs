using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.Guild
{
    class GuildModel : AbstractPestoModel
    {
        public DateTime JoinDate { get; set; }
        public string Name { get; set; }
        public string OwnerUsername { get; set; }
        public ulong OwnerId { get; set; }
        public DateTime LastConnection { get; set; }
    }
}
