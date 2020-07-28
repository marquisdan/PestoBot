using System.Threading.Tasks;

namespace PestoBot.Api.Common
{
    // ReSharper disable once TypeParameterCanBeVariant
    public interface IPestoApi<T>
    {
        T Load(ulong id);
        Task Save(T model);
    }
}
