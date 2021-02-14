using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BasicServices;
using BasicServices.KeyValueStorage;
using IdentityServer4.AccessTokenValidation;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shared.Api;
using Shared.Api.Swagger;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Queue;
using Shared.Helpers.Security;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Controllers;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Services;
using Transactions.Business.Data;
using Transactions.Business.Services;
using Transactions.Shared;
using SharedApi = Shared.Api;
using SharedHelpers = Shared.Helpers;

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
            var appConfig = Configuration.GetSection("AppConfig").Get<ApplicationSettings>();

            services.AddLogging(logging =>
            {
                logging.AddConfiguration(Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddAzureWebAppDiagnostics(); // TODO: remove for production
            });

            var identity = Configuration.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(
                                            "http://localhost:8080",
                                            "http://localhost:8081",
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
                    options.JwtBearerEvents = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnTokenValidated = async (context) =>
                        {
                            if (context.Principal.IsInteractiveAdmin())
                            {
                                var svc = context.HttpContext.RequestServices.GetService<IImpersonationService>();
                                var userId = context.Principal.GetDoneByID();
                                var merchantID = await svc.GetImpersonatedMerchantID(userId.Value);

                                if (merchantID != null)
                                {
                                    var impersonationIdentity = new ClaimsIdentity(new[]
                                    {
                                            new Claim(Claims.MerchantIDClaim, merchantID.ToString()),
                                            new Claim(ClaimTypes.Role, Roles.Merchant)
                                    });
                                    context.Principal?.AddIdentity(impersonationIdentity);
                                }
                            }
                            else if (context.Principal.IsMerchant())
                            {
                                var svc = context.HttpContext.RequestServices.GetService<IImpersonationService>();
                                var userId = context.Principal.GetDoneByID();
                                var merchantID = await svc.GetImpersonatedMerchantID(userId.Value);

                                if (merchantID != null)
                                {
                                    var midentity = (ClaimsIdentity)context.Principal.Identity;
                                    var midclaim = midentity.FindFirst(Claims.MerchantIDClaim);
                                    if (midclaim != null)
                                    {
                                        midentity.TryRemoveClaim(midclaim);
                                    }

                                    midentity.AddClaim(new Claim(Claims.MerchantIDClaim, merchantID.ToString()));
                                }
                            }
                        }
                    };
                });

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;  // TODO: remove for production

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.Terminal, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal()));
                options.AddPolicy(Policy.TerminalOrMerchantFrontendOrAdmin, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal() || context.User.IsMerchantFrontend() || context.User.IsAdmin()));
                options.AddPolicy(Policy.MerchantFrontend, policy =>
                   policy.RequireAssertion(context => context.User.IsMerchantFrontend()));
                options.AddPolicy(Policy.AnyAdmin, policy =>
                   policy.RequireAssertion(context => context.User.IsAdmin()));
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
                c.SchemaFilter<SwaggerExcludeFilter>();

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
            services.AddScoped<IConsumersService, ConsumersService>();
            services.AddScoped<IItemsService, ItemsService>();
            services.AddScoped<IImpersonationService, ImpersonationService>();
            services.AddTransient<CardTokenController, CardTokenController>();
            services.AddTransient<InvoicingController, InvoicingController>();
            services.AddTransient<PaymentRequestsController, PaymentRequestsController>();

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
            services.AddScoped<IBillingDealService, BillingDealService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentRequestsService, PaymentRequestsService>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<IKeyValueStorage<CreditCardTokenKeyVault>, AzureKeyValueStorage<CreditCardTokenKeyVault>>();

            // integration
            services.Configure<Shva.ShvaGlobalSettings>(Configuration.GetSection("ShvaGlobalSettings"));
            services.Configure<ClearingHouse.ClearingHouseGlobalSettings>(Configuration.GetSection("ClearingHouseGlobalSettings"));
            services.Configure<EasyInvoice.EasyInvoiceGlobalSettings>(Configuration.GetSection("EasyInvoiceGlobalSettings"));

            services.AddSingleton<IAggregatorResolver, AggregatorResolver>();
            services.AddSingleton<IProcessorResolver, ProcessorResolver>();
            services.AddSingleton<IInvoicingResolver, InvoicingResolver>();

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

            services.AddSingleton<NullAggregator, NullAggregator>(serviceProvider =>
            {
                return new NullAggregator();
            });

            services.AddSingleton<EasyInvoice.ECInvoiceInvoicing, EasyInvoice.ECInvoiceInvoicing>(serviceProvider =>
            {
                var chCfg = serviceProvider.GetRequiredService<IOptions<EasyInvoice.EasyInvoiceGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<EasyInvoice.ECInvoiceInvoicing>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.EasyInvoiceRequestsLogStorageTable, cfg.EasyInvoiceRequestsLogStorageTable);

                return new EasyInvoice.ECInvoiceInvoicing(webApiClient, chCfg, logger, storageService);
            });

            services.Configure<RequestResponseLoggingSettings>((options) =>
            {
                options.RequestsLogStorageTable = appConfig.RequestsLogStorageTable;
                options.StorageConnectionString = appConfig.DefaultStorageConnectionString;
            });

            services.AddSingleton<IRequestLogStorageService, RequestLogStorageService>();

            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.EncrKeyForSharedApiKey);
            });

            services.AddSingleton<SharedHelpers.Email.IEmailSender, AzureQueueEmailSender>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                var queue = new AzureQueue(cfg.DefaultStorageConnectionString, cfg.EmailQueueName);
                return new AzureQueueEmailSender(queue, cfg.DefaultStorageConnectionString, cfg.EmailTableName);
            });

            services.AddSingleton<IQueueResolver, QueueResolver>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                var invoiceQueue = new AzureQueue(cfg.DefaultStorageConnectionString, cfg.InvoiceQueueName);
                return new QueueResolver(invoiceQueue);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "TransactionsApi"));

            app.UseRequestResponseLogging();

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

            loggerFactory.CreateLogger("TransactionsApi.Startup").LogInformation("Started");
        }
    }
}
