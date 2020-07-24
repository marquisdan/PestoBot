using System;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
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

        public User(ulong id) : base(id)
        {
            
        }

        public User(string DiscordName)
        {
            
        }

    }
}