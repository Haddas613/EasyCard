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
using Shared.Api.Swagger;
using Swashbuckle.AspNetCore.Filters;
using System.IO;
using SharedApi = Shared.Api;

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
                logging.AddDebug();
                logging.AddAzureWebAppDiagnostics();
            });

            var identity = Configuration.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(
                                            "http://localhost:4200",
                                            "http://localhost:8080",
                                            "https://ecng-profile.azurewebsites.net",
                                            "https://ecng-merchants.azurewebsites.net")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                    });
            });

            services.AddDistributedMemoryCache();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identity.Authority;
                    options.RequireHttpsMetadata = true;
                    options.RoleClaimType = "role";
                    options.NameClaimType = "name";
                    options.EnableCaching = true;
                });

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.Terminal, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal()));
                options.AddPolicy(Policy.TerminalOrMerchantFrontendOrAdmin, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal() || context.User.IsMerchantFrontend() || context.User.IsAdmin()));
                options.AddPolicy(Policy.MerchantFrontend, policy =>
                   policy.RequireAssertion(context => context.User.IsMerchantFrontend()));
            });

            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy
                {
                    ProcessDictionaryKeys = false
                }
            };

            services.AddControllers(opts =>
            {
                // Adding global custom validation filter
                opts.Filters.Add(new ValidateModelStateFilter());
            })
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
                options.SerializerSettings.ContractResolver = contractResolver;
                options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;

                // Note: do not use options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; - use [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)] attribute in place
            });

            //Required for all infrastructure json serializers such as GlobalExceptionHandler to follow camelCase convention
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = contractResolver
            };

            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Disables [ApiController] automatic bad request result for invalid models
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddSwaggerExamplesFromAssemblyOf<Swagger.CreateTransactionRequestExample>();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "EasyCard Transactions API",
                });

                c.ExampleFilters();

                c.SchemaFilter<EnumSchemaFilter>();

                //c.DocumentFilter<PolymorphismDocumentFilter<Models.Transactions.CreateTransactionRequest>>();
                //c.SchemaFilter<PolymorphismSchemaFilter<Models.Transactions.CreateTransactionRequest>>();

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // add Security information to each operation for OAuth2
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                // if you're using the SecurityRequirementsOperationFilter, you also need to tell Swashbuckle you're using OAuth2
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = ParameterLocation.Header,
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey
                });
            });

            // DI
            services.AddDbContext<Merchants.Business.Data.MerchantsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("MerchantsConnection")));
            services.AddScoped<IMerchantsService, MerchantsService>();
            services.AddScoped<ITerminalsService, TerminalsService>();

            services.AddSingleton<IExternalSystemsService, ExternalSystemService>(serviceProvider =>
            {
                return new ExternalSystemService(Path.Combine(AppContext.BaseDirectory, "external-systems.json"));
            });

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
            services.Configure<Shva.ShvaGlobalSettings>(Configuration.GetSection("ShvaGlobalSettings"));
            services.Configure<ClearingHouse.ClearingHouseGlobalSettings>(Configuration.GetSection("ClearingHouseGlobalSettings"));

            services.AddSingleton<IAggregatorResolver, AggregatorResolver>();
            services.AddSingleton<IProcessorResolver, ProcessorResolver>();

            services.AddSingleton<Shva.ShvaProcessor, Shva.ShvaProcessor>(serviceProvider =>
            {
                var shvaCfg = serviceProvider.GetRequiredService<IOptions<Shva.ShvaGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<Shva.ShvaProcessor>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.ShvaRequestsLogStorageTable, cfg.ShvaRequestsLogStorageTable);

                return new Shva.ShvaProcessor(webApiClient, shvaCfg, logger, storageService);
            });

            services.AddSingleton<ClearingHouse.ClearingHouseAggregator, ClearingHouse.ClearingHouseAggregator>(serviceProvider =>
            {
                var chCfg = serviceProvider.GetRequiredService<IOptions<ClearingHouse.ClearingHouseGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<ClearingHouse.ClearingHouseAggregator>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.ClearingHouseRequestsLogStorageTable, cfg.ClearingHouseRequestsLogStorageTable);
                var tokenSvc = new WebApiClientTokenService(webApiClient.HttpClient, chCfg);

                return new ClearingHouse.ClearingHouseAggregator(webApiClient, logger, chCfg, tokenSvc, storageService);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection")));

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

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            loggerFactory.CreateLogger("Startup").LogInformation("Started");
        }
    }
}
