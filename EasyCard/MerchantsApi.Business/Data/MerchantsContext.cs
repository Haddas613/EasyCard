using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace MerchantsApi.Business.Data
{
    public class MerchantsContext : DbContext
    {
        public MerchantsContext(DbContextOptions<MerchantsContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
