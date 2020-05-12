using System;
using PestoBot.Database.Models.Guild;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories.Guild
{
    class GuildSettingsRepository : AbstractPestoRepository<GuildSettingsModel>
    {

        public GuildSettingsRepository()
        {
            TableName = "GuildSettings";
        }

        public virtual async void SaveNewGuildSettings(ulong guildId)
        {
            var settingsModel = new GuildSettingsModel()
            {
                Created = DateTime.Now,
                Modified = DateTime.Now
            };
            await InsertAsync(settingsModel);
        }

    }
}
