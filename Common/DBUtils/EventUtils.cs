using System;
using System.Collections.Generic;
using System.Text;
using PestoBot.Database.Repositories.SpeedrunEvent;
using PestoBot.Entity.Event;

namespace PestoBot.Common.DBUtils
{
    internal static class EventUtils
    {
        internal static EventEntity GetNextEventForGuild(ulong guildId)
        {
            var model = new EventRepository().GetNextEventForGuild(guildId).Result;
            return new EventEntity(model);
        }
    }
}
