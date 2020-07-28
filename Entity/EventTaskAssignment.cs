using System;
using Discord;
using Discord.WebSocket;
using PestoBot.Api.Common;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Entity.Common;

namespace PestoBot.Entity
{
    public class EventTaskAssignment : AbstractPestoEntity<EventTaskAssignmentModel>
    {

        private readonly EventTaskAssignmentApi _api;
        private User _assignedUser;

        #region Public Properties

        public string Content
        {
            get => Model.ReminderText;
            set => Model.ReminderText = value;
        }

        public DateTime LastSent => Model.LastReminderSent;

        public ReminderTypes Type
        {
            get => (ReminderTypes) Model.AssignmentType;
            set => Model.AssignmentType = (int) value;
        }

        public ulong AssignmentId { get; set; }

        public User AssignedUser
        {
            get => _assignedUser;
            set => SetUser(value);
        }

        public ulong GuildId
        {
            get => Model.GuildId;
            private set => Model.GuildId = value;
        }

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

        public void SetUser(User user)
        {
            if (IsUserAssignable())
            {
                _assignedUser = user;
                Model.UserId = _assignedUser.Id;
                return;
            }
            throw new ArgumentException("User not assignable or does not exist");
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