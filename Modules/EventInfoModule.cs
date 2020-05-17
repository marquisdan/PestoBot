using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PestoBot.Database.Models.Guild;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories;
using PestoBot.Database.Repositories.Guild;

namespace PestoBot.Modules
{
    public class EventInfoModule : ModuleBase
    {
        private const string DefaultEvntName = "";

        //Display information about a spedrunning event
        [Command("EventInfo")]
        [Alias("AboutEvent","GetEventInfo")]
        [Summary("Gets information about a speedrunning event")]
        public async Task GetEventInfo(string evntName = DefaultEvntName)
        {
            var EventRepo = new EventRepository();
            var guildId = Context.Guild.Id;
            var evnt = evntName == DefaultEvntName ? EventRepo.GetNextEventForGuild(guildId).Result : EventRepo.GetEventByName(evntName).Result;

            await ReplyAsync(null, false, GetEventInfoEmbed(evnt).Build());
        }

        //Construct EmbedBuilder for an event
        private EmbedBuilder GetEventInfoEmbed(EventModel evnt)
        {
            var guild = new GuildRepository().GetAsync(evnt.GuildId).Result;
            var eb = new EmbedBuilder()
            {
                Title = $"{evnt.Name}",
                Color = Color.Green,
                Description = $"An event by {guild.Name}"
            };
            eb.AddField("Starts", evnt.StartDate == DateTime.MinValue ? "Not Set" : evnt.StartDate.ToShortDateString(), true);
            eb.AddField("Ends", evnt.StartDate == DateTime.MinValue ? "Not Set" : evnt.EndDate.ToShortDateString(),true);
            eb.AddField("Schedule", string.IsNullOrEmpty(evnt.ScheduleUrl) ? "Not Yet Available" : $"[View Here]({evnt.ScheduleUrl})", true);
           
            if (!string.IsNullOrEmpty(evnt.ApplicationUrl))
            {
                eb.AddField("Application",$"[Apply Here]({evnt.ApplicationUrl})", true);
                if (ShouldShowScheduleDueDate(evnt))
                {
                    eb.AddField("Deadline", evnt.ScheduleCloseDate.ToShortDateString(), true);
                }
            }


            if (!string.IsNullOrEmpty(evnt.Charity))
            {
                var fieldText = new StringBuilder();
                fieldText.Append($"{evnt.Charity} ");
                if (!string.IsNullOrEmpty(evnt.CharityUrl))
                {
                    fieldText.Append($"[Website]({evnt.CharityUrl}) ");
                }

                if (!string.IsNullOrEmpty(evnt.DonationUrl))
                {
                    fieldText.Append($"[Donate]({evnt.CharityUrl})");
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
