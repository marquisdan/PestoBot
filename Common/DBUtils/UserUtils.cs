using Discord.WebSocket;
using PestoBot.Database.Repositories.Common;
using PestoBot.Entity;

namespace PestoBot.Common.DBUtils
{
    internal static class UserUtils
    {
        internal static User GetUserByDiscordName(DiscordSocketClient client, string userName, string discriminator)
        {
            var userModel = new UserRepository().GetUserByDiscordName(userName, discriminator).Result;
            var user = userModel == null ? new User(GenerateNewUser(client, userName, discriminator)) : new User(userModel);

            return user;
        }

        private static User GenerateNewUser(DiscordSocketClient client, string userName, string discriminator)
        {
            var socketUser = client.GetUser(userName, discriminator);

            return new User(socketUser.Id)
            {
                Username = client.GetUser(userName, discriminator).Username,
                Discriminator = userName
            };
        }
    }
}
