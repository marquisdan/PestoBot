using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories.SpeedrunEvent
{
    class ReminderRepository : AbstractPestoRepository<ReminderModel>
    {
        public ReminderRepository()
        {
            TableName = "Reminder";
        }

        public async Task<List<ReminderModel>> GetListOfReminders(ReminderTypes type)
        {
            switch (type)
            {
                case ReminderTypes.Task: return await GetTaskReminders();
                case ReminderTypes.Run: break;
                case ReminderTypes.Project: break;
                case ReminderTypes.Debug: break;
                default: throw new NotImplementedException();
            }
            return new List<ReminderModel>();
        }

        public async Task<List<ReminderModel>> GetTaskReminders()
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Select * from {TableName} where Type = @Type";
                var dynamicParams = new DynamicParameters(new ReminderModel() { Type = (int) ReminderTypes.Task});
                var data = await db.QueryAsync<ReminderModel>(query, dynamicParams);
                return data.ToList();
            }
        }
    }
}
