﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Business
{
    public class ServiceBase<T, Tk>
        where T : class, IEntityBase<Tk>
    {
        private readonly DbContext dbContext;
        private readonly DbSet<T> entities;

        public ServiceBase(DbContext context)
        {
            this.dbContext = context;
            this.entities = context.Set<T>();
        }

        public virtual async Task UpdateEntity(T entity, IDbContextTransaction dbTransaction = null)
        {
            T exist = this.entities.Find(entity.GetID());

            this.dbContext.Entry(exist).CurrentValues.SetValues(entity);

            await this.dbContext.SaveChangesAsync();
        }

        public virtual async Task CreateEntity(T entity, IDbContextTransaction dbTransaction = null)
        {
            entities.Add(entity);

            await dbContext.SaveChangesAsync();
        }

        public virtual IDbContextTransaction BeginDbTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.RepeatableRead)
        {
            return dbContext.Database.BeginTransaction(isolationLevel);
        }

        public virtual async Task ReloadEntity(T entity)
        {
            await dbContext.Entry(entity).ReloadAsync();
        }
    }
}
