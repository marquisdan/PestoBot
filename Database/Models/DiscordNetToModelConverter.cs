using System;
using Discord.WebSocket;
using SpeedathonBot.Database.Models.Guild;
using SpeedathonBot.Database.Repositories.Guild;

namespace SpeedathonBot.Database.Models
{
    internal static class DiscordNetToModelConverter
    {
        //This is a somewhat hacky band-aid approach to a problem that I did not have time to properly solve
        //It will likely live in the code forever because that's how these things go.
        //Good luck future coder.

        public static GuildModel SocketGuildToModel(SocketGuild socketGuild)
        {
            var now = DateTime.Now;
            var model = new  GuildModel()
            {
                Name = socketGuild.Name,
                Id = socketGuild.Id,
                OwnerUsername = socketGuild.Owner.Username,
                OwnerId = socketGuild.OwnerId
            };

            //Check for a existing guild in the DB and sync
            var existingGuild = new GuildRepository().GetAsync(model.Id).Result;
            if (existingGuild != null)
            {
                model.Created = existingGuild.Created;
                model.Modified = existingGuild.Modified;
                model.JoinDate = existingGuild.JoinDate;
                model.LastConnection = existingGuild.LastConnection;
            }

            return model;
        }
    }
}
