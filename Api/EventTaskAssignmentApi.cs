using System;
using System.Collections.Generic;
using PestoBot.Api.Common;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.Event;

namespace PestoBot.Api
{
    class EventTaskAssignmentApi : AbstractPestoApi<EventTaskAssignmentModel>
    {
        private EventTaskAssignmentModel _model;
        private readonly EventTaskAssignmentRepository _repo;

        internal EventTaskAssignmentApi()
        {
            _model = new EventTaskAssignmentModel
            {
                Created = DateTime.Now
            };

            _repo = new EventTaskAssignmentRepository();
        }

        internal EventTaskAssignmentApi(EventTaskAssignmentModel model)
        {
            _model = model;
            _repo = new EventTaskAssignmentRepository();
        }

        internal  List<EventTaskAssignmentModel> GetAllAssignmentsByType(ReminderTypes type)
        {
            return _repo.GetAssignmentsByType(type).Result;
        }
    }
}