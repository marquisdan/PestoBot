using System;
using Discord;
using Discord.WebSocket;
using PestoBot.Api;
using PestoBot.Api.Common;
using PestoBot.Api.Event;
using PestoBot.Common;
using PestoBot.Database.Models.Event;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Entity.Common;

namespace PestoBot.Entity.Event
{
    public class EventTaskAssignment : AbstractPestoEntity<EventTaskAssignmentModel>
    {

        private readonly EventTaskAssignmentApi _api;
        private User _assignedUser;
        private EventEntity _event;

        #region Public Properties

        public string Content
        {
            get => Model.ReminderText;
            set => Model.ReminderText = value;
        }

        public ReminderTypes Type
        {
            get => (ReminderTypes) Model.AssignmentType;
            set => Model.AssignmentType = (int) value;
        }

        public DateTime ProjectDueDate
        {
            get => Model.ProjectDueDate; 
            set => Model.ProjectDueDate = value;
        }

        public DateTime TaskStartTime
        {
            get => Model.TaskStartTime;
            set => Model.TaskStartTime = value;
        }

        public DateTime LastSent => Model.LastReminderSent;

        public User AssignedUser
        {
            get => _assignedUser;
            set => SetUser(value);
        }

        public EventEntity Event
        {
            get => _event;
            set => SetEvent(value);
        }

        public ulong EventId
        {
            get; 
            protected internal set;
        }

        //TODO Set accessors for EventTaskId, EventId

        #endregion

        public EventTaskAssignment()
        {
            _api = new EventTaskAssignmentApi();
            Model = new EventTaskAssignmentModel();
        }

        public EventTaskAssignment(ulong id)
        {
            _api = new EventTaskAssignmentApi();
            Model = _api.Load(id);
            Content = Model.ReminderText;
        }

        protected override IPestoApi<EventTaskAssignmentModel> GetApi()
        {
            return _api;
        }

        private void SetUser(User user)
        {
            if (IsUserAssignable())
            {
                _assignedUser = user;
                Model.UserId = _assignedUser.Id;
                return;
            }
            throw new ArgumentException("User not assignable or does not exist");
        }

        private void SetEvent(EventEntity evnt)
        {
            //TODO Validate this ok
            _event = evnt;
        }

        private bool IsUserAssignable()
        {
            throw new NotImplementedException();
        }

        public async void Send(DiscordSocketClient client, ulong channelId)
        {
            var connectionLogChannel = (IMessageChannel)client.GetChannel(channelId);
            await connectionLogChannel.SendMessageAsync(Model.ReminderText);
            Model.LastReminderSent = DateTime.Now;
            Save();
        }
    }
}