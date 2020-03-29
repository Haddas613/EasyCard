using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Shared.Api.Validation;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shared.Api;
using Transactions.Business.Data;
using Microsoft.EntityFrameworkCore;
using Merchants.Business.Services;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Shared.Business.Security;
using Transactions.Business.Services;
using Shared.Helpers.KeyValueStorage;
using BasicServices.KeyValueStorage;
using Shared.Integration.Models;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Services;
using Microsoft.Extensions.Options;
using Shared.Helpers;
using BasicServices;
using Transactions.Shared;
using Shared.Helpers.Security;
using IdentityServer4.AccessTokenValidation;

namespace Transactions.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging =>
            {
                logging.AddConfiguration(Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddAzureWebAppDiagnostics();
            });

            var identity = Configuration.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();

            services.AddCors();

            services.AddDistributedMemoryCache();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identity.Authority;
                    options.RequireHttpsMetadata = true;
                    options.RoleClaimType = "role";
                    options.NameClaimType = "name";
                    options.ApiName = "transactions_api";
                    options.EnableCaching = true;
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.Terminal, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal()));
                options.AddPolicy(Policy.TerminalOrMerchantFrontend, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal() || context.User.IsMerchantFrontend()));
                options.AddPolicy(Policy.MerchantFrontend, policy =>
                   policy.RequireAssertion(context => context.User.IsMerchantFrontend()));
            });

            services.AddControllers(opts =>
            {
                // Adding global custom validation filter
                opts.Filters.Add(new ValidateModelStateFilter());
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;

                // Note: do not use options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; - use [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)] attribute in place
            });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Disables [ApiController] automatic bad request result for invalid models
                options.SuppressModelStateInvalidFilter = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Transactions API",
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            // DI
            services.AddDbContext<Merchants.Business.Data.MerchantsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("MerchantsConnection")));
            services.AddScoped<IMerchantsService, MerchantsService>();
            services.AddScoped<ITerminalsService, TerminalsService>();

            services.Configure<ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<AzureKeyVaultSettings>(Configuration.GetSection("AzureKeyVaultTokenStorageSettings"));

            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            services.AddDbContext<TransactionsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ITransactionsService, TransactionsService>();
            services.AddScoped<ICreditCardTokenService, CreditCardTokenService>();
            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<IKeyValueStorage<CreditCardTokenKeyVault>, AzureKeyValueStorage<CreditCardTokenKeyVault>>();

            // integration
            services.Configure<Shva.Configuration.ShvaGlobalSettings>(Configuration.GetSection("ShvaGlobalSettings"));
            services.Configure<ClearingHouse.ClearingHouseGlobalSettings>(Configuration.GetSection("ClearingHouseGlobalSettings"));

            services.AddSingleton<IAggregatorResolver, AggregatorResolver>();
            services.AddSingleton<IProcessorResolver, ProcessorResolver>();

            services.AddSingleton<Shva.ShvaProcessor, Shva.ShvaProcessor>(serviceProvider =>
            {
                var shvaCfg = serviceProvider.GetRequiredService<IOptions<Shva.Configuration.ShvaGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<Shva.ShvaProcessor>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.ShvaRequestsLogStorageTable);

                return new Shva.ShvaProcessor(webApiClient, shvaCfg, logger, storageService);
            });

            services.AddSingleton<ClearingHouse.ClearingHouseAggregator, ClearingHouse.ClearingHouseAggregator>(serviceProvider =>
            {
                var chCfg = serviceProvider.GetRequiredService<IOptions<ClearingHouse.ClearingHouseGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<ClearingHouse.ClearingHouseAggregator>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.ClearingHouseRequestsLogStorageTable);
                var tokenSvc = new WebApiClientTokenService(webApiClient.HttpClient, chCfg);

                return new ClearingHouse.ClearingHouseAggregator(webApiClient, logger, chCfg, tokenSvc, storageService);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(GlobalExceptionHandler.HandleException);

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transactions Api V1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
