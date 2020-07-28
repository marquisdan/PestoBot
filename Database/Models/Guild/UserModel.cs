using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Models.Guild
{
    public class UserModel : AbstractPestoModel
    {
        public string Username { get; set; }
        public string DiscordName { get; set; }
        public bool IsVolunteer { get; set; }
    }
}