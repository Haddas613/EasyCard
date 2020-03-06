using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business
{
    public class ServiceBase<T>
        where T : class, IEntityBase
    {
        private readonly DbContext dbContext;
        private readonly DbSet<T> entities;

        public ServiceBase(DbContext context)
        {
            this.dbContext = context;
            this.entities = context.Set<T>();
        }

        public virtual async Task UpdateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null)
        {
            T exist = this.entities.Find(entity.GetID());

            this.dbContext.Entry(exist).CurrentValues.SetValues(entity);

            await this.dbContext.SaveChangesAsync();
        }

        public virtual async Task CreateEntity(T entity, Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction dbTransaction = null)
        {
            entities.Add(entity);

            await dbContext.SaveChangesAsync();
        }
    }
}
