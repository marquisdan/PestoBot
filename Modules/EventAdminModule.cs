using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories;

namespace PestoBot.Modules
{
    public class EventAdminModule : ModuleBase
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
                var msg = $"Event {evntName} created successfully!";
                await ReplyAsync(TextUtils.GetSuccessText(msg));
            }
            catch (Exception e)
            {
                var msg = $"Event not saved!: {e.Message}";
                await ReplyAsync(TextUtils.GetErrorText(msg));
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

        //Add Schedule Link to Event
        [Command("SetSchedule")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddHoraro", "AddSchedule", "SetScheduleUrl","SetScheduleLink", "AddScheduleToEvent")]
        [Summary("Adds Schedule url to Event")]
        public async Task AddScheduleUrl(string eventName, string url)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            evnt.ScheduleUrl = url;

            await UpdateEventWithReply(repo, evnt);
        }

        //Add Submissions link to event
        [Command("SetSubmissionsUrl")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddSubmissions", "AddSignup", "AddSubmissionsLinkToEvent","SetEventSubmissionsLink","SetEventSignup","SetSubLink","SetAppLink")]
        [Summary("Adds a signup link for submissions to an Event")]
        public async Task AddSubmissionsUrl(string eventName, string url)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            evnt.ApplicationUrl = url;

            await UpdateEventWithReply(repo, evnt);
        }

        //Add Charity to Event 
        [Command("SetCharity")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddCharity", "AddCharityToEvent","SetEventCharity")]
        [Summary("Adds a charity to an event")]
        public async Task AddCharityName(string eventName, string charityName)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            evnt.Charity = charityName;

            await UpdateEventWithReply(repo, evnt);
        }

        //Add CharityUrl to Event 
        [Command("SetCharityUrl")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("AddCharityUrl", "AddCharityLink", "AddCharityLinkToEvent", "SetCharityLink")]
        [Summary("Adds a link for a charity to an event")]
        public async Task AddCharityUrl(string eventName, string url)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            evnt.CharityUrl = url;

            await UpdateEventWithReply(repo, evnt);
        }

        //Set Doation Link
        [Command("SetDonationLink")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetDonos")]
        [Summary("Adds a donation link to event")]
        public async Task AddDonationUrl(string eventName, string url)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            evnt.DonationUrl = url;

            await UpdateEventWithReply(repo, evnt);
        }

        //Set Start Date
        [Command("SetStartDate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetEventStartDate", "SetStart")]
        [Summary("Adds a start date to an event")]
        public async Task AddStartDate(string eventName, string startDate)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            if (DateTime.TryParse(startDate, out var parsedStartDate))
            {
                evnt.StartDate = parsedStartDate;
                await UpdateEventWithReply(repo, evnt);
            }
            else
            {
                var msg = $"Did not recognize {startDate} as a date!";
                await ReplyAsync(TextUtils.GetErrorText(msg));
            }
        }

        //Set End Date
        [Command("SetEndDate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetEventEndDate", "SetEnd")]
        [Summary("Adds an end date to an event")]
        public async Task AddEndDate(string eventName, string endDate)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            if (DateTime.TryParse(endDate, out var parsedEndDate))
            {
                evnt.EndDate = parsedEndDate;
                await UpdateEventWithReply(repo, evnt);
            }
            else
            {
                var msg = $"Did not recognize {endDate} as a date!";
                await ReplyAsync(TextUtils.GetErrorText(msg));
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
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
            if (DateTime.TryParse(startDate, out var parsedStartDate) && DateTime.TryParse(endDate, out var parsedEndDate))
            {
                evnt.StartDate = parsedStartDate;
                evnt.EndDate = parsedEndDate;
                await UpdateEventWithReply(repo, evnt);
            }
            else
            {
                var msg = "There was an error understanding one of the dates!";
                await ReplyAsync(TextUtils.GetErrorText(msg));
            }
        }
        //Set Application due date
        [Command("SetSubmissionsDueDate")]
        [RequireUserPermission(GuildPermission.Administrator)]
        [Alias("SetSubmissionDeadline", "SetApplicationDeadline","SetApplicationDueDate", "SetSubmissionsDueDate")]
        [Summary("Adds an end date to an event")]
        public async Task AddSubmissionDeadline(string eventName, string deadlineDate)
        {
            var repo = new EventRepository();
            var evnt = await repo.GetGuildEventByName(eventName, Context.Guild.Id);
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
                var msg = $"Event [{evnt.Name}] successfully updated!";
                await ReplyAsync(TextUtils.GetSuccessText(msg));
            }
            catch
            {
                var msg = $"Event [{evnt.Name}] could not be updated";
                await ReplyAsync(TextUtils.GetErrorText(msg));
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
