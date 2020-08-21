using System;
using System.IO;
using System.Net.Mime;
using System.Threading.Tasks;
using Castle.Core.Internal;
using Discord.Commands;
using PestoBot.Common;
using PestoBot.Common.CustomPreconditions;
using PestoBot.Common.DBUtils;
using PestoBot.Services;

namespace PestoBot.Modules
{
    public class ReminderServiceModule : ModuleBase
    {

        [RequireBotAdmin]
        [Command("StartReminders")]
        [Alias("BeginReminders")]
        [Summary("Starts reminder service for an event. Will not start a service if it is already running.")]
        public async Task StartReminders(string eventName = "")
        {
            if (eventName.IsNullOrEmpty())
            {
                eventName = GetDefaultEventName();
            }

            var svc = new MarathonReminderService(Context.Channel.Id, eventName);
            try
            {

                svc.Start();

                await ReplyAsync(TextUtils.GetInfoText($"Starting reminders for {eventName}"));
            }
            catch (Exception e)
            {
                await ReplyAsync(TextUtils.GetErrorText($"Unable to start reminder service: {e.Message}"));
            }
        }

        [RequireBotAdmin]
        [Command("StopReminders")]
        [Alias("EndReminders")]
        [Summary("Ends the current reminder service for an event. Takes up to 2 minutes to end the service.")]
        public async Task EndReminders(string eventName = "")
        {
            if (eventName.IsNullOrEmpty())
            {
                eventName = GetDefaultEventName();
            }

            if (MarathonReminderService.DoesLockFileExist(eventName) == false)
            {
                await ReplyAsync(TextUtils.GetWarnText($"Reminder Service for {eventName} is not running"));
                return;
            }

            try
            {
                MarathonReminderService.EndRun(eventName);
                await ReplyAsync(TextUtils.GetInfoText($"Kill command sent for {eventName}, please allow up to 2 minutes for service to end"));
            }
            catch (Exception e)
            {
                await ReplyAsync(TextUtils.GetErrorText($"Unable to end run: {e.Message}"));
            }
        }

        [RequireBotAdmin]
        [Command("Behind")]
        [Summary("Sets event X minutes behind schedule")]
        public async Task SetBehind(int minutesBehind = 0, string eventName = "")
        {
            if (eventName.IsNullOrEmpty())
            {
                eventName = GetDefaultEventName();
            }

            SetMinutesBehind(eventName, minutesBehind);

            await ReplyAsync(TextUtils.GetSuccessText($"{eventName} set to {minutesBehind} minutes behind schedule"));
        }

        [RequireBotAdmin]
        [Command("Ahead")]
        [Summary("Sets event X minutes ahead of schedule")]
        public async Task SetAhead(int minutesBehind = 0, string eventName = "")
        {
            minutesBehind *= -1;  //Being ahead is negative minutes behind. Idk math or something 

            if (eventName.IsNullOrEmpty())
            {
                eventName = GetDefaultEventName();
            }

            SetMinutesBehind(eventName, minutesBehind);

            await ReplyAsync(TextUtils.GetSuccessText($"{eventName} set to {Math.Abs(minutesBehind)} minutes ahead of schedule"));
        }

        [RequireBotAdmin]
        [Command("OnTime")]
        [Summary("Sets event to be on schedule")]
        public async Task SetOnTime(string eventName = "")
        {
            if (eventName.IsNullOrEmpty())
            {
                eventName = GetDefaultEventName();
            }

            SetMinutesBehind(eventName, 0);

            await ReplyAsync(TextUtils.GetSuccessText($"{eventName} set to be on time"));
        }

        [Command("AreWeOnTime")]
        [Alias("AreWeBehind", "AreWeAhead")]
        [Summary("Returns the number of minutes ahead or behind schedule")]
        public async Task GetBehindTime(string eventName = "")
        {
            if (eventName.IsNullOrEmpty())
            {
                eventName = GetDefaultEventName();
            }

            var path = $"{eventName}{Path.DirectorySeparatorChar}{MarathonReminderService.MinutesBehindFileName}";
            string txt ="";
            if (File.Exists(path))
            {
                txt = File.ReadAllText(path);
            }

            var exists = int.TryParse(txt, out int minutes);

            if (exists == false || minutes == 0)
            {
                await ReplyAsync($"{eventName} is currently on time");
                return;
            }
            var aheadBehind = minutes > 0 ? "behind" : "ahead of";
            await ReplyAsync(TextUtils.GetInfoText($"{eventName} is currently {Math.Abs(minutes)} minutes {aheadBehind} schedule"));
        }

        private void SetMinutesBehind(string eventName, int minutes)
        {
            MarathonReminderService.SetMinutesBehind(eventName,minutes);
        }

        private string GetDefaultEventName()
        {
            //return EventUtils.GetNextEventForGuild(Context.Guild.Id).Name;
            //return "Debug";
            return "MidwestSpeedfest2020"; //TODO fix DB issue
        }
    }
}