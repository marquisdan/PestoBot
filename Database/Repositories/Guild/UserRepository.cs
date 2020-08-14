using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using NUnit.Framework.Constraints;
using PestoBot.Database.Models.Guild;

namespace PestoBot.Database.Repositories.Common
{
    internal class UserRepository : AbstractPestoRepository<UserModel>
    {
        public UserRepository()
        {
            TableName = "User";
            AutoIncrementId = false;
        }

        public async Task<UserModel> GetUserByDiscordName(string discordName, string discriminator)
        {
            using IDbConnection db = new SqliteConnection(LoadConnectionString());

            var query = $"Select * from {TableName} where UserName=@UserName AND Discriminator = @Discriminator";
            var dynamicParams = new { UserName = discordName, Discriminator = discriminator};

            var result = await db.QuerySingleOrDefaultAsync<UserModel>(query, dynamicParams);

            if (result == null)
            {
                Console.WriteLine($"{TableName} with id [{discordName}] could not be found.");
            }

            return result;
        }
    }
}