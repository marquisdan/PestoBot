using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PestoBot.Common;
using PestoBot.Common.CustomPreconditions;
using PestoBot.Database.Models.DebugModel;
using PestoBot.Database.Repositories.DebugRepo;
using PestoBot.Database.Repositories.Guild;
using Serilog;

namespace PestoBot.Modules
{
    public class DebugModule : ModuleBase
    {
        [RequireOwner]
        [Command("LoadPeople")]
        [Alias("GetPeople")]
        [Summary("Gets context of person table for testing purposes")]
        public async Task LoadPeopleAsync()
        {
            var people = await new DebugRepository().GetAllAsync();
            var sb = new StringBuilder();
            sb.AppendLine("People in test table 'DebugPerson'");
            sb.AppendLine($"|{"ID",-5}|{"FirstName",-15}|{"LastName",-15}|");
            foreach (var person in people)
            {
                sb.AppendLine($"|{person.Id,-5}|{person.FirstName,-15}|{person.LastName,-15}|");
            }

            await ReplyAsync(sb.ToString());
        }

        [RequireOwner]
        [Command("SavePerson")]
        [Alias("AddPerson")]
        [Summary("Adds a person to the test table 'DebugPerson'")]
        public async Task AddPersonAsync(string first, string last)
        {
            var repo = new DebugRepository();
            var person = new DebugPersonModel
            {
                FirstName = first,
                LastName = last
            };

            try
            {
                await repo.InsertAsync(person);
                await ReplyAsync($"Person [{person.FirstName}], [{person.LastName}] added to DB successfully!");
            }
            catch (Exception e)
            {
                Log.Error(e.Message);
                await ReplyAsync($"Could not save person: {e.Message}");
            }
        }

        [RequireOwner]
        [Command("ShowGuildTable")]
        [Summary("Selects data in a table")]
        public async Task ShowGuildTable(int numRows)
        {
            var repo = new GuildRepository();
            var results = repo.GetFirstXRows(numRows).Result.ToList();
            var sb = new StringBuilder();
            sb.AppendLine($"First {numRows} rows of Guild Table");
            sb.AppendLine($"Table: Guild | Rows In Table: {repo.GetAllAsync().Result.Count()}");

            foreach (var result in results)
            {
                sb.AppendLine($"|Id:{result.Id}| Name: {result.Name}| Join Date: {result.JoinDate}| Last Connection: {result.LastConnection}|");
            }

            await ReplyAsync(sb.ToString());
        }

        [RequireOwner]
        [Command("GetRawTableData")]
        [Alias("LogRawTableData")]
        [Summary("Logs raw table data")]
        public async Task LogRawTableData(string tableName)
        {
            var result = await new DebugRepository().GetRawDataFromTable(tableName);
            Console.WriteLine(result.ToAsyncEnumerable());
            Console.WriteLine(result.ToString());
        }

        [RequireOwner]
        [Command("DeleteAllDatabaseData")]
        [Alias("ClearDatatbase")]
        [Summary("Don't do this")]
        public async Task DeleteAllDatabaseData()
        {
            var repo = new DebugRepository();
            await repo.DeleteAllDatabaseData();
        }

        [RequireOwner]
        [Command("SendPM")]
        [Summary("Sends a private message to a user")]
        public async Task SendPMToUser(ulong userId, string msg)
        {
            Log.Information("SendPM Command received.");
            var usr = Context.Client.GetUserAsync(userId).Result;
            Log.Information($"{usr.Username} found");
            await usr.SendMessageAsync(msg);
            Log.Information("PM Sent");
        }

        [RequireOwner]
        [Command("ServerTime")]
        [Alias("GetServerTime", "ShowServerTime")]
        [Summary("Gets server time")]
        public async Task GetServerTime()
        {
            await ReplyAsync(DateTime.Now.ToString("MMMM dd, yyyy HH:mm:ss tt zz"));
        }

        [Command("Ping")]
        [RequireBotAdmin]
        [Summary("Get bot latency")]
        public async Task GetPing()
        {
            await ReplyAsync("Pong! 🏓 **" + ((DiscordSocketClient) Context.Client).Latency + "ms**");
        }

        //[Command("EnableGooby")]
        //[RequireBotAdmin]
        //[Summary("Go to bed Gooby")]
        //public async Task EnableGoobyReminder()
        //{
        //    var ಠ_ಠ = "hello";
        //    Log.Information(ಠ_ಠ);
        //}
    }
}
