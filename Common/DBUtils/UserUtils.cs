using System;
using System.Collections.Generic;
using System.Text;
using Discord.WebSocket;
using PestoBot.Database.Repositories.Common;
using PestoBot.Entity;

namespace PestoBot.Common.DBUtils
{
    internal static class UserUtils
    {
        public static User GetUserByDiscordName(DiscordSocketClient client, string UserName)
        {
            var repo = new UserRepository();
            throw new NotImplementedException();
        }
    }
}
