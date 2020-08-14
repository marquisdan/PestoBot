using System;
using PestoBot.Api.Common;
using PestoBot.Database.Models.Guild;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Api
{
    internal class UserApi : AbstractPestoApi<UserModel>
    {
        private UserModel _model;
        private readonly UserRepository _repo;

        public UserApi()
        {
            _model = new UserModel()
            {
                Created = DateTime.Now
            };

            _repo = new UserRepository();
        }

        public UserApi(UserModel model)
        {
            _model = model;
            _repo = new UserRepository();
        }

    }
}