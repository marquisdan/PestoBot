using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Dapper;
using Microsoft.Data.Sqlite;
using PestoBot.Database.Models.Common;

namespace PestoBot.Database.Repositories.Common
{
    class GlobalSettingsRepository : AbstractPestoRepository<GlobalSettingsModel>
    {
        public GlobalSettingsRepository()
        {
            TableName = "GlobalSettings";
        }

        public async Task<GlobalSettingsModel> GetAllGlobalSettings()
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Select * from {TableName}";
                return await db.QueryFirstAsync<GlobalSettingsModel>(query);
            }
        }

        public async Task<bool> InitGlobalSettings()
        {
            var isSuccess = false;
            IEnumerable<GlobalSettingsModel> existing;
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Select * from {TableName}";
                existing = await db.QueryAsync<GlobalSettingsModel>(query);
            }
            if (existing.IsNullOrEmpty())
            {
                await InsertAsync(new GlobalSettingsModel
                {
                    Created = DateTime.Now,
                    Modified = DateTime.Now,
                    DebugRemindersEnabled = false,
                    DebugReminderHour = 20,
                    DebugReminderMinutes = 0
                });
                isSuccess = true;
            }

            return isSuccess;
        }

        public async Task<bool> AreDebugRemindersEnabled()
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Select DebugRemindersEnabled from {TableName}";
                return await db.QueryFirstAsync<bool>(query);
            }
        }
    }
}