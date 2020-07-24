using System;
using System.Collections.Generic;
using System.Text;
using PestoBot.Api.Common;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Common;
using PestoBot.Database.Repositories.SpeedrunEvent;

namespace PestoBot.Api
{
    class ReminderApi : AbstractPestoApi<ReminderModel>
    {
        private ReminderModel _model;
        private readonly ReminderRepository _repo;
        public ReminderApi()
        {
            _model = new ReminderModel()
            {
                Created = DateTime.Now
            };
            _repo = new ReminderRepository();
        }

        public ReminderApi(ReminderModel model)
        {
            _model = model;
            _repo = new ReminderRepository();
        }

        protected override IPestoRepository<ReminderModel> GetRepo()
        {
            return _repo;
        }

        public override ReminderModel Load(ulong id)
        {
            return base.Load(_repo, id);
        }


        //Get Event

        //Get Project

        //Get Task

        //Get user

        //Get Due Date


    }
}
