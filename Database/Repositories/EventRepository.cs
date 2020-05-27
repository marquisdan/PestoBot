using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Dapper;
using Microsoft.Data.Sqlite;
using PestoBot.Database.Models.Guild;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories
{
    public class EventRepository: AbstractPestoRepository<EventModel>
    {
        public EventRepository()
        {
            TableName = "Event";
        }

        public virtual async Task SaveNewEvent(EventModel evnt)
        {
            evnt.Created = evnt.Created == DateTime.MinValue ? DateTime.Now : evnt.Created;
            evnt.Modified = DateTime.Now;
            await InsertAsync(evnt);
        }

        public virtual async Task<List<EventModel>> GetAllEventsByGuild(ulong guildId)
        {
            var query = $"Select * from {TableName} where {GuildIdFk} = @GuildId";
            var dynamicParams = new DynamicParameters(new EventModel() { GuildId = guildId });
            return await GetEvents(query, dynamicParams);
        }

        private static async Task<List<EventModel>> GetEvents(string query, DynamicParameters dynamicParams)
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var data = await db.QueryAsync<EventModel>(query, dynamicParams);
                return data.ToList();
            }
        }

        public virtual async Task<List<EventModel>> GetAllEventsByCreatorId(ulong userId)
        {
            var query = $"Select * from {TableName} where CreatorId = @CreatorId";
            var dynamicParams = new DynamicParameters(new EventModel { CreatorId = userId });
            return await GetEvents(query, dynamicParams);
        }

        public virtual async Task<EventModel> GetGuildEventByName(string name, ulong guildId)
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Select * from {TableName} where Name = @Name AND {GuildIdFk} = @GuildId";
                var dynamicParams = new DynamicParameters(new EventModel() {Name = name, GuildId = guildId});
                return await db.QueryFirstAsync<EventModel>(query, dynamicParams);
            }

        }
        #nullable enable
        public virtual async Task<EventModel>? GetEventByName(string name)
        
        {

            var query = $"Select * from {TableName} where Name = @Name";
            var dynamicParams = new DynamicParameters(new EventModel() { Name = name });

            return await GetNullableNextEvent(query, dynamicParams);
        }

        public virtual async Task<EventModel>? GetNextEventForGuild(ulong guildId)
        {
            var query = $"Select * from {TableName} where {GuildIdFk} = @GuildId order by StartDate desc";
            var dynamicParams = new DynamicParameters(new EventModel() { GuildId = guildId });

            return await GetNullableNextEvent(query, dynamicParams);
        }

        private static async Task<EventModel> GetNullableNextEvent(string query, DynamicParameters dynamicParams)
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                try
                {
                    var data = await db.QueryAsync<EventModel>(query, dynamicParams);
                    return data.First();
                }
                catch (InvalidOperationException e)
                {
                    return null;
                }
            }
        }


#nullable disable
        public virtual async Task<int> RemoveEvent(string name, ulong guildId)
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Delete from Event where name = @Name and {GuildIdFk} = @GuildId";
                var dynamicParams = new DynamicParameters(new EventModel() { Name = name, GuildId = guildId });
                var result = await db.ExecuteAsync(query, dynamicParams);
                return result;
            }
        }
    }
}