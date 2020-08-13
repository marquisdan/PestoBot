using PestoBot.Database.Models.Guild;
using PestoBot.Entity.Common;

namespace PestoBot.Entity
{
    public class User : AbstractPestoEntity<UserModel>
    {
        #region Public Properties

        public string Username
        {
            get => Model.Username; 
            set => Model.Username = value;
        }

        public string DiscordName
        {
            get => Model.DiscordName;
            set => Model.DiscordName = value;
        }
        public bool IsVolunteer
        {
            get => Model.IsVolunteer;
            set => Model.IsVolunteer = value;
        }

        #endregion

        public User()
        {

        }

        public User(ulong id) : base(id)
        {
            
        }

        public User(string DiscordName)
        {
            
        }

        public User(User user)
        {
            Model.Id = user.Id;
            Model.Created = user.Created;
            Model.Modified = user.Modified;
            Model.Username = user.Username;
            Model.DiscordName = user.DiscordName;
            Model.IsVolunteer = user.IsVolunteer;
        }

        public User(UserModel model)
        {
            Model = model;
        }

    }
}