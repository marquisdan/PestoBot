using System;
using PestoBot.Api.Event;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Entity.Common;

namespace PestoBot.Entity.Event
{
    public class EventEntity: AbstractPestoEntity<EventModel> 
    {
        private readonly EventApi _api;

        public string Name
        {
            get => Model.Name; 
            set => Model.Name =value;
        }

        public EventEntity(EventModel model)
        {
            _api = new EventApi();
            Model = model;
        }

        public EventEntity(string name)
        {
            _api = new EventApi();
            var existingModel = _api.GetEventByName(name);
            Model = existingModel ?? new EventModel()
            {
                Created = DateTime.Now,
                Modified = DateTime.Now,
                Name = name
            };
        }
    }
}