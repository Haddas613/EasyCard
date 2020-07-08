using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace Shared.Business
{
    public interface IServiceBase<T, Tk>
        where T : class, IEntityBase<Tk>
    {
        Task UpdateEntity(T entity, IDbContextTransaction dbTransaction = null);

        Task CreateEntity(T entity, IDbContextTransaction dbTransaction = null);

        IDbContextTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.RepeatableRead);

        Task ReloadEntity(T entity);
    }
}
