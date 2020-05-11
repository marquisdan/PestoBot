using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Serilog;
//using SpeedathonBot.Database.Models.Debug;
//using SpeedathonBot.Database.Repositories;
//using SpeedathonBot.Database.Repositories.Guild;

namespace SpeedathonBot.Modules
{
    public class DebugModule : ModuleBase
    {
        //[RequireOwner]
        //[Command("LoadPeople")]
        //[Alias("GetPeople")]
        //[Summary("Gets context of person table for testing purposes")]
        //public async Task LoadPeopleAsync()
        //{
        //    var people = await new DebugRepository().GetAllAsync();
        //    var sb = new StringBuilder();
        //    sb.AppendLine("People in test table 'DebugPerson'");
        //    sb.AppendLine($"|{"ID",-5}|{"FirstName",-15}|{"LastName",-15}|");
        //    foreach (var person in people)
        //    {
        //        sb.AppendLine($"|{person.Id, -5}|{person.FirstName, -15}|{person.LastName, -15}|");
        //    }

        //    await ReplyAsync(sb.ToString());
        //}

        //[RequireOwner]
        //[Command("SavePerson")]
        //[Alias("AddPerson")]
        //[Summary("Adds a person to the test table 'DebugPerson'")]
        //public async Task AddPersonAsync(string first, string last)
        //{
        //    var repo = new DebugRepository();
        //    var person = new DebugModel
        //    {
        //        FirstName = first,
        //        LastName = last
        //    };

        //    try
        //    {
        //        await repo.InsertAsync(person);
        //        await ReplyAsync($"Person [{person.FirstName}], [{person.LastName}] added to DB successfully!");
        //    }
        //    catch(Exception e)
        //    {
        //        Log.Error(e.Message);
        //        await ReplyAsync($"Could not save person: {e.Message}");
        //    }
        //}

        //[RequireOwner]
        //[Command("ShowGuildTable")]
        //[Summary("Selects data in a table")]
        //public async Task ShowGuildTable(int numRows)
        //{
        //    var repo = new GuildRepository();
        //    var results = repo.GetFirstXRows(numRows).Result.ToList();
        //    var sb = new StringBuilder();
        //    sb.AppendLine($"First {numRows} rows of Guild Table");
        //    sb.AppendLine($"Table: Guild | Rows In Table: {repo.GetAllAsync().Result.Count()}");

        //    foreach (var result in results)
        //    {
        //        sb.AppendLine($"|Id:{result.Id}| Name: {result.Name}| Join Date: {result.JoinDate}| Last Connection: {result.LastConnection}|");
        //    }

        //    await ReplyAsync(sb.ToString());
        //}

        //[RequireOwner]
        //[Command("GetRawTableData")]
        //[Alias("LogRawTableData")]
        //[Summary("Logs raw table data")]
        //public async Task LogRawTableData(string tableName)
        //{
        //    var result = await new DebugRepository().GetRawDataFromTable(tableName);
        //    Console.WriteLine(result.ToAsyncEnumerable());
        //    Console.WriteLine(result.ToString());
        //}

        //[RequireOwner]
        //[Command("DeleteAllDatabaseData")]
        //[Alias("ClearDatatbase")]
        //[Summary("Don't do this")]
        //public async Task DeleteAllDatabaseData()
        //{
        //    var repo = new DebugRepository();
        //    await repo.DeleteAllDatabaseData();
        //}
    }
}
