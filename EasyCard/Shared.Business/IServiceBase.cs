using System.Threading.Tasks;

namespace Shared.Business
{
    public interface IServiceBase<T, Tk>
        where T : class, IEntityBase<Tk>
    {
        Task UpdateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null);

        Task CreateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null);
    }
}
