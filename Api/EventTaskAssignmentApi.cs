using System;
using System.Collections.Generic;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.SpeedrunEvent;

namespace PestoBot.Api.Common
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

        internal static List<EventTaskAssignmentModel> GetAllAssignmentsByType(ReminderTypes type)
        {
            throw new NotImplementedException();
        }
    }
}