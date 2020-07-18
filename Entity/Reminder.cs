using System;
using Discord;
using Discord.WebSocket;
using PestoBot.Api;
using PestoBot.Common;
using PestoBot.Database.Models.SpeedrunEvent;
using PestoBot.Entity.Common;

namespace PestoBot.Entity
{
    public class Reminder : AbstractPestoEntity<ReminderModel>
    {
        public string Content
        {
            get => Model.Content;
            set => Model.Content = value;
        }

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

        public User AssignedUser
        {
            get => _assignedUser;
            set { _assignedUser = value;
                Model.UserId = _assignedUser.Id;
            }
        }

        private User _assignedUser;
        //public Assignment assignment {get;}
        //public Guild guild { get; }

        public Reminder()
        {
            Api = new ReminderApi<ReminderModel>();
            Model = new ReminderModel();
        }

        public Reminder(ulong id)
        {
            Api = new ReminderApi<ReminderModel>();
            Model = Api.Load(id);
            Content = Model.Content;
        }

        public void SetUser(User user)
        {
            if (IsUserAssignable())
            {
                _assignedUser = user;
                return;
            }
            throw new ArgumentException("User not assignable or does not exist");
        }

        private bool IsUserAssignable()
        {
            throw new NotImplementedException();
        }

        public DateTime GetLastSent()
        {
            return Model.LastSent;
        }

        public async void Send(DiscordSocketClient client, ulong channelId)
        {
            var connectionLogChannel = (IMessageChannel)client.GetChannel(channelId);
            await connectionLogChannel.SendMessageAsync(Model.Content);
        }
    }
}