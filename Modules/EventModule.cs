using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories;

//using SpeedathonBot.Database.Models.SpeedrunEvent;
//using SpeedathonBot.Database.Repositories;

namespace PestoBot.Modules
{
    public class EventModule : ModuleBase
    {
        //Create Event
        [Command("CreateEvent")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddEvent")]
        [Summary("Creates a new event")]
        public async Task CreateEvent(string evntName)
        {
            var evnt = new EventModel()
            {
                Created = DateTime.Now,
                GuildId = Context.Guild.Id,
                Name = evntName,
                CreatorUsername = Context.Message.Author.Username,
                CreatorId = Context.Message.Author.Id
            };
            try
            {
                await new EventRepository().SaveNewEvent(evnt);
                await ReplyAsync($"Event {evntName} created successfully!");
            }
            catch (Exception e)
            {
                await ReplyAsync($"Event not saved!: {e.Message}");
            }

        }

        //View All Events by Guild
        [Command("ListEvents")]
        [Summary("List all events created by this discord server")]
        public async Task ListEventsByGuild()
        {
            var results = new EventRepository().GetAllEventsByGuild(Context.Guild.Id).Result;
            if (results == null || results.Count == 0)
            {
                await ReplyAsync("No events created yet!");
            }
            else
            {
                var eb = new EmbedBuilder()
                {
                    Color = Color.Green,
                    Title = "All Events",
                };

                foreach (var result in results)
                {
                    var sb = new StringBuilder();
                    sb.Append($"\\|| Start: [Placeholder] \\|| End: [Placeholder] ");
                    sb.Append($"\\||Schedule: [Link]({result.ScheduleUrl})");
                    eb.AddField(result.Name, sb.ToString());
                }
                await ReplyAsync(null, false, eb.Build());
            }
        }

        //Add Schedule Link to Event
        [Command("AddScheduleToEvent")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddHoraro", "AddSchedule")]
        [Summary("Adds Schedule url to Event")]
        public async Task AddScheduleUrl(string eventName, string url)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            evnt.ScheduleUrl = url;

            await UpdateEventWithReply(repo, evnt);
        }

        //Add Submissions link to event
        [Command("AddSubmissionsLinkToEvent")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddSubmissions", "AddSignup")]
        [Summary("Adds a signup link for submissions to an Event")]
        public async Task AddSubmissionsUrl(string eventName, string url)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            evnt.ApplicationUrl = url;

            await UpdateEventWithReply(repo, evnt);
        }

        //Add Charity to Event 
        [Command("AddCharityToEvent")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddCharity")]
        [Summary("Adds a charity to an event")]
        public async Task AddCharityName(string eventName, string charityName)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            evnt.Charity = charityName;

            await UpdateEventWithReply(repo, evnt);
        }

        //Add CharityUrl to Event 
        [Command("AddCharityLinkToEvent")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddCharityUrl", "AddCharityLink")]
        [Summary("Adds a link for a charity to an event")]
        public async Task AddCharityUrl(string eventName, string url)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            evnt.CharityUrl = url;

            await UpdateEventWithReply(repo, evnt);
        }

        //Updates event table and sends reply back to channel
        private async Task UpdateEventWithReply(EventRepository repo, EventModel evnt)
        {
            try
            {
                await repo.UpdateAsync(evnt);
                await ReplyAsync($"Event {evnt.Name} updated successfully!");
            }
            catch
            {
                await ReplyAsync($"Event {evnt.Name} could not be updated");
            }
        }


        //Delete an event
        [RequireUserPermission(GuildPermission.Administrator)]
        [Command("DeleteEvent")]
        [Alias("RemoveEvent")]
        [Summary("Removes Event")]
        public async Task RemoveEvent(string eventName)
        {
            var numRows = await new EventRepository().RemoveEvent(eventName, Context.Guild.Id);
            switch (numRows)
            {
                case 1:
                    {
                        await ReplyAsync($"Successfully Deleted {eventName}");
                        break;
                    }
                case 0:
                    {
                        await ReplyAsync($"No event named {eventName} found!");
                        break;
                    }
                default:
                    {
                        await ReplyAsync($"Something has gone catastrophically wrong and you deleted {numRows} events. Congrats.");
                        break;
                    }
            }
        }
    }
}
