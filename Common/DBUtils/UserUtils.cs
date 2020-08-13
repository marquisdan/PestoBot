using Discord.WebSocket;
using PestoBot.Database.Repositories.Common;
using PestoBot.Entity;

namespace PestoBot.Common.DBUtils
{
    internal static class UserUtils
    {
        public static User GetUserByDiscordName(DiscordSocketClient client, string discordName)
        {
            var userModel = new UserRepository().GetUserByDiscordName(discordName).Result;
            var user = userModel == null ? new User(GenerateNewUserFromDiscordName(client, discordName)) : new User(userModel);

            return user;
        }

        private static User GenerateNewUserFromDiscordName(DiscordSocketClient client, string discordName)
        {
            return new User
            {
                Username = client.GetUser(discordName, null).Username,
                DiscordName = discordName
            };
        }
    }
}
