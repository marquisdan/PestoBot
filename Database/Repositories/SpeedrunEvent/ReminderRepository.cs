using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using PestoBot.Common;
using PestoBot.Database.Models.Common;
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

        public async Task<List<ReminderModel>> GetRemindersByType(ReminderTypes type)
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Select * from {TableName} where Type = @Type";
                var dynamicParams = new DynamicParameters(new ReminderModel { Type = (int) type});
                var data = await db.QueryAsync<ReminderModel>(query, dynamicParams);
                return data.ToList();
            }
        }

        public async Task<IPestoModel> GetAssignmentForReminder(ReminderModel model)
        {

            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                string tableName;
                string query;
                DynamicParameters dynamicParams;
                IPestoModel data;

                switch ((ReminderTypes) model.Type)
                {
                    case ReminderTypes.Task:
                        tableName = "MarathonTaskAssignment";
                        query = $"Select * from {tableName} where Id = @id";
                        dynamicParams = new DynamicParameters(new MarathonTaskAssignmentModel
                            {Id = model.AssignmentId});
                        data = await db.QueryFirstAsync<MarathonTaskAssignmentModel>(query, dynamicParams);
                        return (MarathonTaskAssignmentModel) data;

                    case ReminderTypes.Project:
                        tableName = "MarathonTableAssignment";
                        query = $"Select * from {tableName} where Id = @id";
                        dynamicParams = new DynamicParameters(new MarathonProjectAssignmentModel
                            {Id = model.AssignmentId});
                        data = await db.QueryFirstAsync<MarathonProjectAssignmentModel>(query, dynamicParams);
                        return (MarathonProjectAssignmentModel) data;
                    default: throw new ArgumentException($"No assignment for this type of reminder: {model.Type}");
                }
            }
        }

    }
}