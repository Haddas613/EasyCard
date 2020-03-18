﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Transactions.Api.Mapping;
using Transactions.Business.Data;
using Transactions.Business.Services;

namespace Transactions.Tests.Fixtures
{
    public class TransactionsFixture : IDisposable
    {
        public TransactionsContext TransactionsContext { get; private set; }

        public HttpContextAccessorWrapperFixture HttpContextAccessorWrapper { get; private set; }

        public TransactionsService TransactionsService { get; private set; }

        public CreditCardTokenService CreditCardTokenService { get; private set; }

        public IMapper Mapper { get; private set; }

        public TransactionsFixture()
        {
            var opts = new DbContextOptionsBuilder<TransactionsContext>();
            opts.UseSqlServer($"Data Source=(LocalDB)\\MSSQLLocalDB;Initial Catalog=TEST_TransactionsDatabase-{Guid.NewGuid().ToString().Substring(0, 6)};Integrated Security=True");
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
