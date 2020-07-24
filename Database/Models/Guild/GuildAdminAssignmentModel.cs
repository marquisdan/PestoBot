using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.Guild
{
    public class GuildAdminAssignmentModel : AbstractPestoModel
    {
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
    }
}
