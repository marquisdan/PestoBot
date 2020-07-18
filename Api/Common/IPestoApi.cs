using PestoBot.Database.Models.Common;

namespace PestoBot.Api.Common
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IPestoApi<T>
    {
        T Load(ulong id);
        void Save(IPestoModel model);
    }
}
