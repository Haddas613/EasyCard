﻿using AutoMapper;
using Merchants.Api.Mapping;
using Merchants.Business.Data;
using Merchants.Business.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Shared.Tests.Configuration;
using Shared.Tests.Fixtures;
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

        public IMapper Mapper { get; private set; }

        public HttpContextAccessorWrapperFixture HttpContextAccessorWrapper { get; private set; }

        public MerchantsFixture()
        {
            var opts = new DbContextOptionsBuilder<MerchantsContext>();

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

            MerchantsContext = new MerchantsContext(opts.Options, HttpContextAccessorWrapper);
            MerchantsContext.Database.EnsureCreated();

            MerchantsService = new MerchantsService(MerchantsContext, HttpContextAccessorWrapper);
            TerminalsService = new TerminalsService(MerchantsContext, HttpContextAccessorWrapper);

            var myProfile = new AutoMapperProfile();
            var configuration = new MapperConfiguration(cfg => cfg.AddProfile(myProfile));
            Mapper = new Mapper(configuration);
        }

        public void Dispose()
        {
            if (MerchantsContext != null)
            {
                MerchantsContext.Database.EnsureDeleted();
            }
        }
    }
}
