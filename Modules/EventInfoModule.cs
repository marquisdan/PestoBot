using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories;
using PestoBot.Database.Repositories.Guild;
using PestoBot.Services;


namespace PestoBot.Modules
{
    public class EventInfoModule : ModuleBase
    {
        //Display information about a spedrunning event
        [Command("EventInfo")]
        [Alias("AboutEvent","GetEventInfo")]
        [Summary("Gets information about a speedrunning event")]
        public async Task GetEventInfo(string evntName = "")
        {
            var EventRepo = new EventRepository();
            var guildId = Context.Guild.Id;
            var evnt = evntName == string.Empty ? EventRepo.GetNextEventForGuild(guildId).Result : EventRepo.GetEventByName(evntName).Result;

            if (evnt == null)
            {
                var msg = evntName == string.Empty ? "No events created yet!" : "That event does not exist!";
                await ReplyAsync(TextUtils.GetWarnText(msg));
            }
            else
            {
                await ReplyAsync(null, false, GetEventInfoEmbed(evnt).Build());
            }
        }

        //View all open events
        [Command("ListEvents")]
        [Summary("List all open events")]
        public async Task ListEvents()
        {
            var results = new EventRepository().GetAllOpenEvents().Result;
            if (results == null || results.Count == 0)
            {
                var msg = "No events created yet!";
                await ReplyAsync(TextUtils.GetWarnText(msg));
            }
            else
            {
                await ReplyWithEventListEmbed("All Events", results);
            }
        }

        //View All Events by Guild
        [Command("ListServerEvents")]
        [Summary("List all events created by this discord server")]
        public async Task ListEventsByGuild()
        {
            var results = new EventRepository().GetAllEventsByGuild(Context.Guild.Id).Result;
            if (results == null || results.Count == 0)
            {
                var msg = "No events created yet!";
                await ReplyAsync(TextUtils.GetWarnText(msg));
            }
            else
            {
                await ReplyWithEventListEmbed("All Events", results);
            }
        }

        //View all commands by user
        [Command("ListMyEvents")]
        [Alias("GetAllEventsByCreatorId")]
        [Summary("Shows all events you have made")]
        public async Task ListEventsByOwner()
        {
            var author = Context.Message.Author;
            var results = new EventRepository().GetAllEventsByCreatorId(author.Id).Result;
            if (results == null || results.Count == 0)
            {
                var msg = "You have not created any events yet!";
                await ReplyAsync(TextUtils.GetWarnText(msg));
            }
            else
            {
                await ReplyWithEventListEmbed($"Events by {author.Username}", results);
            }
        }

        //Creates an embed with a list of events and replies with it.
        private async Task ReplyWithEventListEmbed(string embedTitle, List<EventModel> results)
        {
            var eb = new EmbedBuilder()
            {
                Color = Color.Green,
                Title = embedTitle,
                Description = $"For detailed information on a specific event use **[EventInfo \"*Event Name*\"]**"
            };

            foreach (var result in results)
            {
                var sb = new StringBuilder();
                sb.Append($"**Start:** " + (result.StartDate == DateTime.MinValue ? "Not set yet!" : result.StartDate.ToShortDateString()));
                sb.Append($" **End:** " + (result.EndDate == DateTime.MinValue ? "Not set yet!" : result.EndDate.ToShortDateString()));
                if (!string.IsNullOrEmpty(result.ApplicationUrl))
                {
                    sb.Append($"\r\n**Applications:** [Link]({result.ApplicationUrl}) **Deadline:** ");
                    if (result.ScheduleCloseDate != DateTime.MinValue)
                    {
                        sb.Append($" **Deadline: ** {result.ScheduleCloseDate.ToShortDateString()}");
                    }
                    sb.Append(result.ScheduleCloseDate == DateTime.MinValue ? "Not set yet!" : result.ScheduleCloseDate.ToShortDateString());
                }
                if (!string.IsNullOrEmpty(result.ScheduleUrl))
                {
                    sb.AppendLine($"\r\n**Schedule:** [Link]({result.ScheduleUrl})");

                }
                eb.AddField(result.Name, sb.ToString());
            }

            await ReplyAsync(null, false, eb.Build());
        }

        //Construct EmbedBuilder for an event
        private EmbedBuilder GetEventInfoEmbed(EventModel evnt)
        {
            var config = ConfigService.BuildConfig();
            var guild = new GuildRepository().GetAsync(evnt.GuildId).Result;
            var eb = new EmbedBuilder()
            {
                Title = $"{evnt.Name}",
                Color = Color.Green,
            };

            if (evnt.GuildId != ulong.Parse(config.GetSection("SpecialGuilds").GetSection("BotSandbox").Value))
            {
                eb.Footer = new EmbedFooterBuilder().WithText($"An event by {guild.Name}");
            }

            if (!(evnt.StartDate == DateTime.MinValue))
            {
                eb.AddField("Starts", evnt.StartDate.ToString(TextUtils.EmbedDateFormat), true);
            }
            if (!(evnt.EndDate == DateTime.MinValue))
            {
                eb.AddField("Ends", evnt.EndDate.ToString(TextUtils.EmbedDateFormat), true);
            }
            if (!string.IsNullOrEmpty(evnt.ScheduleUrl))
            {
                eb.AddField("Schedule", string.IsNullOrEmpty(evnt.ScheduleUrl) ? "Not Yet Available" : $"[View Here]({evnt.ScheduleUrl})", true);
            }
            
            if (!string.IsNullOrEmpty(evnt.ApplicationUrl))
            {
                eb.AddField("Application",$"[Apply Here]({evnt.ApplicationUrl})", true);
                if (ShouldShowScheduleDueDate(evnt))
                {
                    eb.AddField("Deadline", evnt.ScheduleCloseDate.ToString(TextUtils.EmbedDateFormat), true);
                }
            }

            if (!string.IsNullOrEmpty(evnt.Charity))
            {
                var fieldText = new StringBuilder();
                fieldText.Append($"{evnt.Charity} ");
                if (!string.IsNullOrEmpty(evnt.CharityUrl))
                {
                    fieldText.Append($"| [Website]({evnt.CharityUrl}) ");
                }

                if (!string.IsNullOrEmpty(evnt.DonationUrl))
                {
                    fieldText.Append($"| [Donate]({evnt.CharityUrl}) |");
                }

                eb.AddField("Charity", fieldText.ToString());
            }

            return eb;
        }

        private bool ShouldShowScheduleDueDate(EventModel evnt)
        {
            return evnt.ScheduleCloseDate != DateTime.MinValue &&
                   evnt.ScheduleCloseDate > DateTime.Now;
        }
    }
}
