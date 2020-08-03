using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories.Event
{ 
    internal class EventTaskAssignmentRepository : AbstractPestoRepository<EventTaskAssignmentModel>
    {
        internal EventTaskAssignmentRepository()
        {
            TableName = "EventTaskAssignment";
        }

        public async Task<List<EventTaskAssignmentModel>> GetAssignmentsByType(ReminderTypes type)
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = $"Select * from {TableName} where AssignmentType = @AssignmentType";
                var dynamicParams = new DynamicParameters(new EventTaskAssignmentModel { AssignmentType = (int)type });
                var data = await db.QueryAsync<EventTaskAssignmentModel>(query, dynamicParams);
                return data.ToList();
            }
        }
        
    }
}
