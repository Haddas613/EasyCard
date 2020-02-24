using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business
{
    public class ServiceBase<T> where T : class, IEntityBase
    {
        readonly DbContext dbContext;
        readonly DbSet<T> entities;

        public ServiceBase(DbContext context)
        {
            this.dbContext = context;
            this.entities = context.Set<T>();
        }

        public async Task UpdateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null)
        {
            T exist = this.entities.Find(entity.GetID());

            this.dbContext.Entry(exist).CurrentValues.SetValues(entity);

            await this.dbContext.SaveChangesAsync();
        }

        public async Task CreateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null)
        {
            throw new NotImplementedException();
        }
    }
}
