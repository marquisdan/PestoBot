using System;
using PestoBot.Api.Common;
using PestoBot.Database.Models.Common;

namespace PestoBot.Entity.Common
{
    public abstract class AbstractPestoEntity<T> : IPestoEntity<T> where T: IPestoModel
    {
        protected static T Model;
       // protected IPestoApi<T> Api;

        internal readonly ulong Id;
        public DateTime Created => Model.Created;

        public DateTime Modified
        {
            get => Model.Modified;
            private set => Model.Modified = value;
        }

        protected AbstractPestoEntity()
        {

        }

        protected AbstractPestoEntity(ulong id)
        {
            Id = id;
            Load(GetApi(), id);
        }

        protected virtual IPestoApi<T> GetApi()
        {
            throw new NotImplementedException("Must implement this in derived class");

        }

        protected void Load(IPestoApi<T> api, ulong id)
        {
            Model = api.Load(id);
        }

        public void Load(ulong id)
        {
            Load(GetApi(), id);
        }

        protected void Save(IPestoApi<T> api)
        {
            api.Save(Model);
        }

        public virtual void Save()
        {
            Save(GetApi());
        }
    }
}