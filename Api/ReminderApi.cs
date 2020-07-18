using System;
using System.Collections.Generic;
using System.Text;
using PestoBot.Api.Common;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.SpeedrunEvent;

namespace PestoBot.Api
{
    class ReminderApi<T> : AbstractPestoApi<ReminderModel>
    {
        private ReminderModel Model; 
        public ReminderApi()
        {
            Model = new ReminderModel()
            {
                Created = DateTime.Now
            };
            Repo = new ReminderRepository();
        }

        public ReminderApi(ReminderModel model)
        {
            Model = model;
            Repo = new ReminderRepository();
        }

        //Get Event

        //Get Project

        //Get Task

        //Get user

        //Get Due Date


    }
}
