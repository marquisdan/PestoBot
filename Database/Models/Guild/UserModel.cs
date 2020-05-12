using System;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.Guild
{
    public class UserModel : AbstractPestoModel
    {
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public string Username { get; set; }
        public string DiscordName { get; set; }
        public bool IsVolunteer { get; set; }
    }
}