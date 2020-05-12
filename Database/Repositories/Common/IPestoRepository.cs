using System.Collections.Generic;
using System.Threading.Tasks;

namespace PestoBot.Database.Repositories.Common
{
    public interface IPestoRepository<T>
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task DeleteRowAsync(ulong id);
        Task<T> GetAsync(ulong id);
        Task<int> SaveRangeAsync(IEnumerable<T> list);
        Task UpdateAsync(T t);
        Task InsertAsync(T t);
    }
}
