using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business
{
    public interface IServiceBase<T> where T : class, IEntityBase
    {
        Task UpdateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null);

        Task CreateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null);
    }
}
