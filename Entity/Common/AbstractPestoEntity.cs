using PestoBot.Api.Common;
using PestoBot.Database.Models.Common;

namespace PestoBot.Entity.Common
{
    public abstract class AbstractPestoEntity<T> : IPestoEntity<T> where T: IPestoModel
    {

        protected static T Model;
        protected IPestoApi<T> Api;

        internal readonly ulong Id;

        protected AbstractPestoEntity()
        {
            Api = new AbstractPestoApi<T>();
        }

        protected AbstractPestoEntity(ulong id)
        {
            Id = id;
            Load(id);
        }

        public void Load(ulong id)
        {
            
            Model = (T) Api.Load(id);
        }

        public void Save()
        {
            Api.Save((T) Model);
        }
    }
}