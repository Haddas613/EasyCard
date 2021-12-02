using AutoMapper;
using Common.Models;
using Common.Models.MDBSource;
using DesktopEasyCardConvertorECNG.Mapping;
using DesktopEasyCardConvertorECNG.Models;
using DesktopEasyCardConvertorECNG.Models.Helper;
using DesktopEasyCardConvertorECNG.RapidOneClient;
using MerchantProfileApi.Client;
using MerchantProfileApi.Models.Billing;
using MerchantProfileApi.Models.Terminal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RapidOne;
using RapidOne.Configuration;
using RapidOne.Models;
using Serilog;
using Shared.Api.Models;
using Shared.Helpers;
using Shared.Integration;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Client;
using Transactions.Api.Models.Tokens;

namespace DesktopEasyCardConvertorECNG
{
    class Program
    {
        private static readonly WebApiClient webApiClient = new WebApiClient();

        static int Main(string[] args)
        {
            string content = File.ReadAllText(args[0]);
            AppConfig appConfig = JsonConvert.DeserializeObject<AppConfig>(content);

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("DesktopEasyCardConvertorECNG.Program", LogLevel.Debug)
                    .AddConsole()
                    .AddDebug()
                    .AddSerilog(new LoggerConfiguration().WriteTo.File("log.txt").CreateLogger(), dispose: false);
            });
            Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger<Program>();
            logger.LogInformation("Application started");


            var mapperConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<TerminalProfile>();
            });

            var mapper = mapperConfig.CreateMapper();

            RapidOneGlobalSettings rapidOneGlobalSettings = new RapidOneGlobalSettings
            {

            };

            ConsoleIntegrationRequestLogService requestLogService = new ConsoleIntegrationRequestLogService(loggerFactory.CreateLogger<ConsoleIntegrationRequestLogService>());

            RapidOneInvoicing rapidOneInvoicing = new RapidOneInvoicing(webApiClient, Options.Create(rapidOneGlobalSettings), loggerFactory.CreateLogger<RapidOneInvoicing>(), requestLogService);

            RapidOneService rapidOneService = new RapidOneService(logger, appConfig, rapidOneInvoicing);




            var serviceFactory = new ServiceFactory(appConfig.ECNGAPIKey, appConfig.ECNGEnvironment);


            var metadataMerchantService = serviceFactory.GetMerchantMetadataApiClient();
            var transactionsService = serviceFactory.GetTransactionsApiClient();


            MdbECNGConverter converter = new MdbECNGConverter(logger, appConfig, mapper, rapidOneService, transactionsService, metadataMerchantService);


            return converter.ProcessMdbFile().Result;
        }
    }

}


