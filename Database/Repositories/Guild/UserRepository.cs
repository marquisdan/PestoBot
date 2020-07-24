using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using PestoBot.Database.Models.Guild;

namespace PestoBot.Database.Repositories.Common
{
    internal class UserRepository : AbstractPestoRepository<UserModel>
    {
        public UserRepository()
        {
            TableName = "User";
        }

        public async Task<UserModel> GetUserByDiscordName(string discordName)
        {
            using IDbConnection db = new SqliteConnection(LoadConnectionString());

            var query = $"Select * from {TableName} where DiscordName=@DiscordName";
            var dynamicParams = new {DiscordName = discordName};

            var result = await db.QuerySingleOrDefaultAsync<UserModel>(query, dynamicParams);

            if (result == null)
            {
                Console.WriteLine($"{TableName} with id [{discordName}] could not be found.");
            }

            return result;
        }
    }
}