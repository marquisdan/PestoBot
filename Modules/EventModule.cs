using System;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
//using SpeedathonBot.Database.Models.SpeedrunEvent;
//using SpeedathonBot.Database.Repositories;

namespace SpeedathonBot.Modules
{
    public class EventModule : ModuleBase
    {
    //    //Create Event
    //    [Command("CreateEvent")]
    //    [RequireUserPermission(GuildPermission.Administrator)]
    //    [Alias("AddEvent")]
    //    [Summary("Creates a new event")]
    //    public async Task CreateEvent(string evntName)
    //    {
    //        var evnt = new EventModel()
    //        {
    //            Created = DateTime.Now,
    //            GuildId = Context.Guild.Id,
    //            Name = evntName
    //        };
    //        try
    //        {
    //            await new EventRepository().SaveNewEvent(evnt);
    //            await ReplyAsync($"Event {evntName} created successfully!");
    //        }
    //        catch(Exception e)
    //        {
    //            await ReplyAsync($"Event not saved!: {e.Message}");
    //        }
            
    //    }

    //    //View All Events by Guild
    //    [Command("ListEvents")]
    //    [Summary("List all events created by this discord server")]
    //    public async Task ListEventsByGuild()
    //    {
    //        var results = new EventRepository().GetAllEventsByGuild(Context.Guild.Id).Result;
    //        if (results == null || results.Count == 0) 
    //        {
    //           await ReplyAsync("No events created yet!");
    //        }
    //        else
    //        {
    //            var eb = new EmbedBuilder()
    //            {
    //                Color = Color.Green,
    //                Title = "All Events",
    //            };
                
    //            foreach (var result in results)
    //            {
    //                var sb = new StringBuilder();
    //                sb.Append($"\\|| Start: [Placeholder] \\|| End: [Placeholder] ");
    //                sb.Append($"\\||Schedule: [Link]({result.ScheduleLink})");
    //                eb.AddField(result.Name, sb.ToString());
    //            }
    //            await ReplyAsync(null, false, eb.Build());
    //        }
    //    }

    //    //Add Schedule Link to Event
    //    [Command("AddScheduleToEvent")]
    //    [RequireUserPermission(GuildPermission.Administrator)]
    //    [Alias("AddHoraro", "AddSchedule")]
    //    [Summary("Adds Schedule url to Event")]
    //    public async Task AddScheduleUrl(string eventName, string url)
    //    {
    //        var repo = new EventRepository();
    //        var evnt = await repo.GetEventByName(eventName, Context.Guild.Id);
    //        evnt.ScheduleLink = url;

    //        await repo.UpdateAsync(evnt);
    //    }

    //    //Delete an event
    //    [RequireUserPermission(GuildPermission.Administrator)]
    //    [Command("DeleteEvent")]
    //    [Alias("RemoveEvent")]
    //    [Summary("Removes Event")]
    //    public async Task RemoveEvent(string eventName)
    //    {
    //        var numRows = await new EventRepository().RemoveEvent(eventName, Context.Guild.Id);
    //        switch (numRows)
    //        {
    //            case 1:
    //            {
    //                await ReplyAsync($"Successfully Deleted {eventName}");
    //                break;
    //            }
    //            case 0:
    //            {
    //                await ReplyAsync($"No event named {eventName} found!");
    //                break;
    //            }
    //            default:
    //            {
    //                await ReplyAsync($"Something has gone catastrophically wrong and you deleted {numRows} events. Congrats.");
    //                break;
    //            }
    //        }
    //    }
    }
}
