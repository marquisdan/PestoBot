using System;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using NUnit.Framework.Api;
using PestoBot.Common;
using PestoBot.Database.Models.Guild;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Common;
using Serilog;

namespace PestoBot.Database.Repositories.Guild
{
    class GuildSettingsRepository : AbstractPestoRepository<GuildSettingsModel>
    {
        public GuildSettingsRepository()
        {
            TableName = "GuildSettings";
        }

        internal virtual async void SaveNewGuildSettings(ulong guildId)
        {
            var settingsModel = new GuildSettingsModel()
            {
                Created = DateTime.Now,
                Modified = DateTime.Now,
                GuildId = guildId
            };
            await InsertAsync(settingsModel);
        }

        internal async Task<ulong?> GetReminderChannelForType(ulong guildId, ReminderTypes type)
        {
            ulong? result;
            if (GetReminderChannelForType(type, out var reminderChannelName) == false) return null;

            var query = $"Select {reminderChannelName.ToString()} from {TableName} where GuildId=@GuildId";
            var dynamicParams = new { GuildId = guildId };

            try
            {
                using IDbConnection db = new SqliteConnection(LoadConnectionString());
                result = await db.QueryFirstAsync<ulong?>(query, dynamicParams);
            }
            catch(Exception e)
            {
                result = null;
            }

            return result;
        }

        private static bool GetReminderChannelForType(ReminderTypes type, out StringBuilder reminderChannelName)
        {
            reminderChannelName = new StringBuilder();
            switch (type)
            {
                case ReminderTypes.Run:
                    reminderChannelName.Append("Runner");
                    break;
                case ReminderTypes.Task:
                    reminderChannelName.Append("Task");
                    break;
                case ReminderTypes.Project:
                    reminderChannelName.Append("Project");
                    break;
                case ReminderTypes.DebugTask:
                    reminderChannelName.Append("Debug");
                    break;
                case ReminderTypes.DebugProject:
                    reminderChannelName.Append("Debug");
                    break;
                default:
                    return false;
            }

            reminderChannelName.Append("ReminderChannel");
            return true;
        }
    }
}
