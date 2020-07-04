using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.Sqlite;
using PestoBot.Database.Models.DebugModel;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories.DebugRepo
{
    public class DebugRepository : AbstractPestoRepository<DebugModel>
    {
        public DebugRepository()
        {
            TableName = "DebugPerson";
        }

        public List<DebugModel> LoadPeople()
        {
            //"Using" statement ensures db connection is closed. We do not have to use .dispose() or anything. 
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                //Hard coded query for testing
                var output = db.Query<DebugModel>($"select * from {TableName} ", new DynamicParameters());
                return output.ToList();
            }
        }

        public void SavePerson(DebugModel debug)
        {
            //"Using" statement ensures db connection is closed. We do not have to use .dispose() or anything. 
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                db.Execute($"insert into {TableName} (FirstName, LastName)" +
                           "values (@FirstName, @LastName)", debug);
            }
        }

        public async Task DeleteAllDatabaseData()
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var query = @" Delete from DebugPerson;
                                   Delete from Guild;
                                   Delete from Event;
                                   Delete from EventVolunteerAssignment;
                                   Delete from MarathonProject;
                                   Delete from MarathonProjectAssignmentModel;
                                   Delete from MarathonTask;
                                   Delete from MarathonTaskAssignment;
                                   Delete from ServerAdminAssignment;
                                   Delete from User;";

                db.Execute(query);
            }
        }

        public async Task<IEnumerable<dynamic>> GetRawDataFromTable(string table)
        {
            using (IDbConnection db = new SqliteConnection(LoadConnectionString()))
            {
                var result = db.Query($"Select * from {table}");
                Console.WriteLine(result.ToString());
                return result;
            }
        }

    }

}
