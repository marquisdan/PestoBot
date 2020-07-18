using Microsoft.Extensions.Configuration;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Repositories.Common;
using PestoBot.Services;

namespace PestoBot.Api.Common
{
    public class AbstractPestoApi<T> : IPestoApi<T> where T : IPestoModel
    {
        internal IPestoRepository<T> Repo;
       // internal IPestoModel Model;
        private IConfiguration _config;

        public AbstractPestoApi()
        {
            _config = ConfigService.BuildConfig();
        }

        public virtual T Load(ulong id)
        {
            return Repo.GetAsync(id).Result;
        }

        public virtual void Save(IPestoModel model)
        {
            //See if it exists already
            var existingModel = Repo.GetAsync(model.Id).Result;
            if (existingModel == null)
            {
                //If not, save as anew
                //TODO Add updated/modified here
                Repo.InsertAsync((T) model);
                return;
            }
            //if it exists, insert it
            Repo.UpdateAsync(existingModel);

            throw new System.NotImplementedException();
        }
    }

}