using Merchants.Business.Data;
using Merchants.Business.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Tests.Fixtures
{
    public class MerchantsFixture : IDisposable
    {
        public IMerchantsService MerchantsService { get; private set; }
        public ITerminalsService TerminalsService { get; private set; }
        public MerchantsContext MerchantsContext { get; private set; }

        public MerchantsFixture()
        {
            var opts = new DbContextOptionsBuilder<MerchantsContext>();
            opts.UseSqlServer($"Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=TEST_MerchantsDatabase-{Guid.NewGuid().ToString().Substring(0, 6)};Integrated Security=True");

            MerchantsContext = new MerchantsContext(opts.Options, new HttpContextAccessorWrapperFixture(/*TODO: add roles*/));
            MerchantsContext.Database.EnsureCreated();

            MerchantsService = new MerchantsService(MerchantsContext);
            TerminalsService = new TerminalsService(MerchantsContext);
        }

        public void Dispose()
        {
            if (MerchantsContext != null)
                MerchantsContext.Database.EnsureDeleted();
        }
    }
}
