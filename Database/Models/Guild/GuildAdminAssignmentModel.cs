using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.Guild
{
    public class GuildAdminAssignmentModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
    }
}
