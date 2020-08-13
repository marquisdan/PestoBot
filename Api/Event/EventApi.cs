using System;
using PestoBot.Api.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Database.Repositories.SpeedrunEvent;

namespace PestoBot.Api.Event
{
    internal class EventApi : AbstractPestoApi<EventModel>
    {
        private EventModel _model;
        private EventRepository _repo;

        internal EventApi()
        {
            _model = new EventModel()
            {
                Created = DateTime.Now,
                Modified = DateTime.Now
            };

            _repo = new EventRepository();
        }

        internal EventApi(EventModel model)
        {
            _model = model;
            _repo = new EventRepository();
        }

        internal EventModel GetEventByName(string name)
        {
            return _repo.GetEventByName(name).Result;
        }
    }
}