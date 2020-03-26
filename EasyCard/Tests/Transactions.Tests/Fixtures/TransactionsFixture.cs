using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shared.Tests.Configuration;
using Shared.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Transactions.Api.Mapping;
using Transactions.Api.Services;
using Transactions.Business.Data;
using Transactions.Business.Services;

namespace Transactions.Tests.Fixtures
{
    public class TransactionsFixture : IDisposable
    {
        public TransactionsContext TransactionsContext { get; private set; }

        public HttpContextAccessorWrapperFixture HttpContextAccessorWrapper { get; private set; }

        public ITransactionsService TransactionsService { get; private set; }

        public ICreditCardTokenService CreditCardTokenService { get; private set; }

        public IMapper Mapper { get; private set; }

        public ILogger<Api.Controllers.TransactionsApiController> Logger { get; } = new NullLogger<Api.Controllers.TransactionsApiController>();

        public TransactionsFixture()
        {
            var opts = new DbContextOptionsBuilder<TransactionsContext>();

            var config = new ConfigurationBuilder()
                 .AddJsonFile("config.json")
                 .Build();

            var settings = config.Get<TestSettings>();

            if (settings.UseTemporaryDatabase)
            {
                opts.UseSqlServer(config.GetConnectionString("TemporaryDatabase").Replace("{{uniqueid}}", Guid.NewGuid().ToString().Substring(0, 6)));
            }
            else
            {
                opts.UseSqlServer(config.GetConnectionString("DefaultDatabase"));
            }

            HttpContextAccessorWrapper = new HttpContextAccessorWrapperFixture();

            TransactionsContext = new TransactionsContext(opts.Options, HttpContextAccessorWrapper);
            TransactionsContext.Database.EnsureCreated();

            TransactionsService = new TransactionsService(TransactionsContext, HttpContextAccessorWrapper);
            CreditCardTokenService = new CreditCardTokenService(TransactionsContext);

            var configuration = new MapperConfiguration(cfg => cfg.AddMaps(Assembly.GetAssembly(typeof(Api.Controllers.TransactionsApiController))));
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
