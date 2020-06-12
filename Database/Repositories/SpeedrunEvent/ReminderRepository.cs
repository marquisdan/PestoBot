using System;
using System.Collections.Generic;
using System.Text;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Database.Repositories.SpeedrunEvent
{
    class ReminderRepository : AbstractPestoRepository<ReminderModel>
    {
        public ReminderRepository()
        {
            TableName = "Reminder";
        }
    }
}
