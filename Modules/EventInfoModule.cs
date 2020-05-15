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

            eb.Fields.Add(new EmbedFieldBuilder() {Name = "something", Value = "something else"});

            return eb;
        }
    }
}
