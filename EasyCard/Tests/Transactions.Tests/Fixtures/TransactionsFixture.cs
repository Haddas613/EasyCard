using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Api.Infrastructure.Mapping;
using Transactions.Business.Data;
using Transactions.Business.Services;

namespace Transactions.Tests.Fixtures
{
    public class TransactionsFixture : IDisposable
    {
        public TransactionsContext TransactionsContext { get; private set; }

        public HttpContextAccessorWrapperFixture HttpContextAccessorWrapper { get; private set; }

        public TransactionsService TransactionsService { get; private set; }

        public IMapper Mapper { get; private set; }

        public TransactionsFixture()
        {
            var opts = new DbContextOptionsBuilder<TransactionsContext>();
            opts.UseSqlServer($"Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=TEST_TransactionsDatabase-{Guid.NewGuid().ToString().Substring(0, 6)};Integrated Security=True");
            HttpContextAccessorWrapper = new HttpContextAccessorWrapperFixture();

            TransactionsContext = new TransactionsContext(opts.Options, HttpContextAccessorWrapper);
            TransactionsContext.Database.EnsureCreated();

            TransactionsService = new TransactionsService(TransactionsContext);

            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            Mapper = new Mapper(configuration);
        }

        public void Dispose()
        {
            if (TransactionsContext != null)
            {
                TransactionsContext.Database.EnsureDeleted();
            }
        }
    }
}
