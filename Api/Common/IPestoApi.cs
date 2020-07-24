using System.Threading.Tasks;
using PestoBot.Database.Models.Common;
using PestoBot.Database.Repositories.Common;

namespace PestoBot.Api.Common
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IPestoApi<T>
    {
        T Load(ulong id);
        Task Save(T model);
    }
}
