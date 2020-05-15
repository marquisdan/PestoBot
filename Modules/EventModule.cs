using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories;

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
                await ReplyAsync("You have not created any events yet!");
            }
            else
            {
                await ReplyWithEventListEmbed($"Events by {author.Username}", results);
            }
        }

        //Add Schedule Link to Event
        [Command("AddScheduleToEvent")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddHoraro", "AddSchedule", "SetSchedule")]
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

        //Set Start Date
        [Command("SetStartDate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetEventStartDate, SetStart")]
        [Summary("Adds a start date to an event")]
        public async Task AddStartDate(string eventName, string startDate)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            if (DateTime.TryParse(startDate, out var parsedStartDate))
            {
                evnt.StartDate = parsedStartDate;
                await UpdateEventWithReply(repo, evnt);
            }
            else
            { 
                await ReplyAsync($"Did not recognize {startDate} as a date!");
            }
        }

        //Set End Date
        [Command("SetEndDate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetEventEndDate, SetEnd")]
        [Summary("Adds an end date to an event")]
        public async Task AddEndDate(string eventName, string endDate)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            if (DateTime.TryParse(endDate, out var parsedEndDate))
            {
                evnt.EndDate = parsedEndDate;
                await UpdateEventWithReply(repo, evnt);
            }
            else
            {
                await ReplyAsync($"Did not recognize {endDate} as a date!");
            }
        }

        //Set Start and End Dates
        [Command("SetStartEndDates")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetDates", "SetEventDates")]
        [Summary("Adds start and end dates to event")]
        public async Task AddStartEndDates(string eventName, string startDate, string endDate)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            if (DateTime.TryParse(startDate, out var parsedStartDate) && DateTime.TryParse(endDate, out var parsedEndDate))
            {
                evnt.StartDate = parsedStartDate;
                evnt.EndDate = parsedEndDate;
                await UpdateEventWithReply(repo, evnt);
            }
            else
            {
                await ReplyAsync($"There was an error understanding one of the dates!");
            }
        }
        //Set Application due date
        [Command("SetSubmissionDueDate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetSubmissionDeadline, SetApplicationDeadline,SetApplicationDueDate")]
        [Summary("Adds an end date to an event")]
        public async Task AddSubmissionDeadline(string eventName, string deadlineDate)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
            if (DateTime.TryParse(deadlineDate, out var parsedDeadline))
            {
                evnt.ScheduleCloseDate = parsedDeadline;
                await UpdateEventWithReply(repo, evnt);
            }
            else
            {
                await ReplyAsync($"Did not recognize {deadlineDate} as a date!");
            }
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

        //Creates an embed with a list of events and replies with it.
        private async Task ReplyWithEventListEmbed(string embedTitle, List<EventModel> results)
        {
            var eb = new EmbedBuilder()
            {
                Color = Color.Green,
                Title = embedTitle,
            };

            foreach (var result in results)
            {
                var sb = new StringBuilder();
                sb.Append($"\\||Start: " + (result.StartDate == DateTime.MinValue ? "Not set yet!" : result.StartDate.ToShortDateString()));
                sb.Append($"\\|| End: " + (result.EndDate == DateTime.MinValue ? "Not set yet!" : result.EndDate.ToShortDateString()));
                if (!string.IsNullOrEmpty(result.ApplicationUrl))
                {
                    sb.AppendLine($"\\||[Applications]({result.ApplicationUrl}) Deadline: ");
                    sb.Append(result.ScheduleCloseDate == DateTime.MinValue ? "Not set yet!" : result.ScheduleCloseDate.ToShortDateString());
                }
                if (!string.IsNullOrEmpty(result.ScheduleUrl))
                {
                    sb.AppendLine($"\\||[Schedule]({result.ScheduleUrl})");

                }
                eb.AddField(result.Name, sb.ToString());
            }

            await ReplyAsync(null, false, eb.Build());
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
