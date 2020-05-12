using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using PestoBot.Database.Models.Guild;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories.Guild
{
    internal class GuildRepository : AbstractPestoRepository<GuildModel>
    {
        public GuildRepository()
        {
            TableName = "Guild";
            AutoIncrementId = false;
        }

        public virtual List<GuildModel> GetExistingServers()
        {
            using (IDbConnection db = new SQLiteConnection(LoadConnectionString()))
            {
                var output = db.Query<GuildModel>($"select * from {TableName}");
                return output.ToList();
            }
        }

        public virtual async Task SaveNewServer(GuildModel server)
        {
            server.Created = server.Created == DateTime.MinValue ? DateTime.Now : server.Created;
            server.Modified = DateTime.Now;
            server.LastConnection = server.LastConnection == DateTime.MinValue ? DateTime.Now : server.LastConnection;
            server.JoinDate = server.JoinDate == DateTime.MinValue ? DateTime.Now : server.JoinDate;
            await InsertAsync(server);
            //Also create an entry in the guild settings table
            new GuildSettingsRepository().SaveNewGuildSettings(server.Id);
        }

        public virtual async Task AddServerIfNotAlreadyExisting(GuildModel server)
        {
            if (!ServerExists(server))
            {
                server.LastConnection = DateTime.Now;
                await SaveNewServer(server);
            }
        }

        internal virtual bool ServerExists(GuildModel server)
        {
            return GetExistingServers().Any(x => x.Id == server.Id);
        }

        internal async Task UpdateLastConnectionTime(GuildModel server, DateTime lastConnectionTime)
        {
            server.LastConnection = lastConnectionTime;
            await UpdateAsync(server);
        }
    }
}
