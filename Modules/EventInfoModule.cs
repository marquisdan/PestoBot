using System;
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
