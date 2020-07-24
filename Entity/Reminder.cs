using System;
using Discord;
using Discord.WebSocket;
using PestoBot.Api;
using PestoBot.Api.Common;
using PestoBot.Common;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Entity.Common;

namespace PestoBot.Entity
{
    public class Reminder : AbstractPestoEntity<ReminderModel>
    {

        private readonly ReminderApi _api;
        private User _assignedUser;
        private MarathonTaskAssignment _marathonTaskAssignment;

        #region Public Properties

        public string Content
        {
            get => Model.Content;
            set => Model.Content = value;
        }

        public DateTime LastSent => Model.LastSent;

        public int Interval
        {
            get => Model.Interval;
            set => Model.Interval = value;
        }

        public ReminderTypes Type
        {
            get => (ReminderTypes) Model.Type;
            set => Model.Type = (int) value;
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

        public Reminder()
        {
            _api = new ReminderApi();
            Model = new ReminderModel();
        }

        public Reminder(ulong id)
        {
            _api = new ReminderApi();
            Model = _api.Load(id);
            Content = Model.Content;
        }

        protected override IPestoApi<ReminderModel> GetApi()
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
            await connectionLogChannel.SendMessageAsync(Model.Content);
            Model.LastSent = DateTime.Now;
            Save();
        }
    }
}