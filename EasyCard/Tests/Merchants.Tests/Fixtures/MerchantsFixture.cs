﻿using AutoMapper;
using Merchants.Api.Data;
using Merchants.Api.Mapping;
using Merchants.Business.Data;
using Merchants.Business.Services;
using Merchants.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Shared.Api.Configuration;
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

        public IExternalSystemsService ExternalSystemsService { get; private set; }

        public IImpersonationService ImpersonationService { get; private set; }

        public MerchantsContext MerchantsContext { get; private set; }

        public IOptions<ApplicationSettings> ApplicationSettings { get; private set; }

        public IOptions<ApiSettings> ApiSettings { get; private set; }

        public IMapper Mapper { get; private set; }

        public HttpContextAccessorWrapperFixture HttpContextAccessorWrapper { get; private set; }

        public MerchantsFixture()
        {
            var opts = new DbContextOptionsBuilder<MerchantsContext>();

            var config = new ConfigurationBuilder()
                 .AddJsonFile("config.json")
                 .Build();

            var settings = config.Get<TestSettings>();
            string connectionString = string.Empty;

            if (settings.UseTemporaryDatabase)
            {
                connectionString = config.GetConnectionString("TemporaryDatabase").Replace("{{uniqueid}}", Guid.NewGuid().ToString().Substring(0, 6));
                opts.UseSqlServer(connectionString);
            }
            else
            {
                connectionString = config.GetConnectionString("DefaultDatabase");
                opts.UseSqlServer(connectionString);
            }

            HttpContextAccessorWrapper = new HttpContextAccessorWrapperFixture();

            //All tests in merchant are by default performed from BillingAdmin perspective
            //This can later be overrided in any particular test that does require other role
            HttpContextAccessorWrapper.SetRoleToBillingAdministrator();

            MerchantsContext = new MerchantsContext(opts.Options, HttpContextAccessorWrapper);
            MerchantsContext.Database.EnsureCreated();
            SeedData.EnsureSeedData(connectionString);

            MerchantsService = new MerchantsService(MerchantsContext, HttpContextAccessorWrapper);
            TerminalsService = new TerminalsService(MerchantsContext, HttpContextAccessorWrapper);
            ExternalSystemsService = new ExternalSystemService();

            ImpersonationService = new ImpersonationService(MerchantsContext, HttpContextAccessorWrapper, null);
            ApplicationSettings = Options.Create(new ApplicationSettings());
            ApiSettings = Options.Create(new ApiSettings());

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
