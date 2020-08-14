using PestoBot.Api;
using PestoBot.Api.Common;
using PestoBot.Database.Models.Guild;
using PestoBot.Entity.Common;

namespace PestoBot.Entity
{
    public class User : AbstractPestoEntity<UserModel>
    {
        #region Public Properties

        private UserApi _api = new UserApi();

        public string Username
        {
            get => Model.Username; 
            set => Model.Username = value;
        }

        public string Discriminator
        {
            get => Model.Discriminator;
            set => Model.Discriminator = value;
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
            Model.Discriminator = user.Discriminator;
            Model.IsVolunteer = user.IsVolunteer;
        }

        public User(UserModel model)
        {
            Model = model;
        }

        protected override IPestoApi<UserModel> GetApi()
        {
            return _api;
        }
    }
}