using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PestoBot.Common;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Models.Guild;
using PestoBot.Database.Repositories.Common;
using PestoBot.Database.Repositories.Guild;
using PestoBot.Entity;
using PestoBot.Services;

namespace PestoBot.Api.Common
{
    internal abstract class AbstractPestoApi<T> : IPestoApi<T> where T : IPestoModel

    { 
        private IConfiguration _config;

        protected AbstractPestoApi()
        {
            _config = ConfigService.BuildConfig();
        }

        protected virtual IPestoRepository<T> GetRepo()
        {
            throw new NotImplementedException("Implement this in base class");
        }

        public virtual T Load(ulong id)
        {
            return Load(GetRepo(), id);
        }

        protected virtual T Load(IPestoRepository<T> repo, ulong id)
        {
            return repo.GetAsync(id).Result;
        }

        public virtual async Task Save(T model)
        {
            await Save(GetRepo(), model);
        }

        protected virtual async Task Save(IPestoRepository<T> repo, T model)
        {
            //See if it exists already
            var existingModel = repo.GetAsync(model.Id).Result;
            if (existingModel == null)
            {
                //If it does not exist, save as new
                model.Created = DateTime.Now;
                model.Modified = DateTime.Now;
                await repo.InsertAsync(model);
                return;
            }

            //if it exists, update it
            model.Created = existingModel.Created;
            model.Modified = DateTime.Now;
            await repo.UpdateAsync(model);
        }
    }

    internal class GuildSettingsApi : AbstractPestoApi<GuildSettingsModel>
    {
        private GuildSettingsModel _model;
        private readonly GuildSettingsRepository _repo;

        internal GuildSettingsApi()
        {
            _model = new GuildSettingsModel()
            {
                Created = DateTime.Now
            };

            _repo = new GuildSettingsRepository();
        }

        internal GuildSettingsApi(GuildSettingsModel model)
        {
            model = new GuildSettingsModel();
            _repo = new GuildSettingsRepository();
        }

        public ulong? GetReminderChannelForType(ReminderTypes reminderType)
        {
            return GetReminderChannelForType(_model.GuildId, reminderType);
        }

        public ulong? GetReminderChannelForType(ulong guildId, ReminderTypes reminderType)
        {
            return _repo.GetReminderChannelForType(_model.GuildId, reminderType).Result;
        }

    }
}
      