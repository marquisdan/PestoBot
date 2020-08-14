using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PestoBot.Common;
using PestoBot.Common.CustomPreconditions;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.DebugModel;
using PestoBot.Database.Repositories.DebugRepo;
using PestoBot.Database.Repositories.Guild;
using PestoBot.Database.Repositories.SpeedrunEvent;
using PestoBot.Entity;
using PestoBot.Entity.Event;
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

        #region GlobalSettings
        [RequireOwner]
        [Command("InitGlobals")]
        [Alias("InitGlobalSettings")]
        public async Task InitGlobalSettings()
        {
            await ReplyAsync(GlobalSettings.InitGlobalSettings()
                ? "Settings initialized successfully"
                : "Global settings already initialized. Can't initialize em anymore");
        }
        
        [RequireOwner]
        [Command("GetGlobalSettings")]
        [Alias("GlobalSettings", "ListGlobalSettings", "Globals")]
        [Summary("Get global settings")]
        public async Task GetGlobalSettings()
        {
            var settings = GlobalSettings.GetGlobalSettings();
            var eb = new EmbedBuilder()
            {
                Color = Color.Green,
                Title = "Global Settings",
                Description = "Global Settings for PestoBot"
            };

            foreach (var property in settings.GetType().GetProperties())
            {
                if (property.Name != "Id")
                {
                    eb.AddField(property.Name, property.GetValue(settings), true);
                }
            }

            await ReplyAsync(null, false, eb.Build());
        }

        [RequireOwner]
        [Command("SetDebugReminders")]
        [Alias("EnableDebugReminders", "DebugReminders")]
        [Summary("Enable or Disable Debug Reminders")]
        public async Task SetDebugReminders(string enabled = "")
        {
            if (new List<string>{"true", "1", "on", "false", "0", "off"}.Contains(enabled.ToLower()))
            {
                bool enabledSetting = enabled.ToLower() == "true" || enabled == "1" || enabled.ToLower() == "on";
                var model = new GlobalSettingsModel{DebugRemindersEnabled =  enabledSetting};
                await GlobalSettings.SetGlobalSettings(model);
                await ReplyAsync(TextUtils.GetInfoText($"Setting Debug reminders to {enabledSetting}"));
            }

            var currentVal = GlobalSettings.AreDebugRemindersEnabled().ToString();
            await ReplyAsync(TextUtils.GetInfoText($"Debug reminder value is: {currentVal}"));
        }

        #endregion

        [Command("AddRecurringDebugReminder")]
        [RequireBotAdmin]
        [Summary("Set a recurring debug reminder")]
        public async Task AddRecurringDebugReminder(string msg, string targetUser)
        {
       
        }

        [Command("AddOneTimeDebugReminder")]
        [RequireBotAdmin]
        [Summary("Set a one time debug reminder")]
        public async Task AddOneTimeDebugReminder(string msg, string targetUser, string evnt = "")
        {
            var validUser = ulong.TryParse(targetUser, out var userId);
            if (validUser == false)
            {
                await ReplyAsync($"{targetUser} is not a valid userId");
                return;
            }

            var user = new User(userId);
            var evntToAssign = new EventRepository().GetNextEventForGuild(Context.Guild.Id);

            var reminder = new EventTaskAssignment
            {
                Content = msg,
                Type = ReminderTypes.DebugTask,
                AssignedUser = user
            };
        }

        [RequireOwner]
        [Command("PingUserTest")]
        public async Task PingUserTest(string pingTarget)
        {
            var isId = ulong.TryParse(pingTarget, out var id);
            IUser user;
            if (isId)
            {
                user = Context.Client.GetUserAsync(id).Result;
            }
            else
            {
                var userName = pingTarget.Split('#');
                user = Context.Client.GetUserAsync(userName[0], userName[1]).Result;
            }

            await ReplyAsync($"{user.Mention} test");
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
