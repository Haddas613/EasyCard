using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AspNetCore.Authentication.ApiKey;
using AutoMapper;
using BasicServices;
using BasicServices.KeyValueStorage;
using Bit.Configuration;
using Bit.Services;
using IdentityServer4.AccessTokenValidation;
using InforU;
using Merchants.Business.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
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
using Shared.Api.Configuration;
using Shared.Api.Swagger;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Configuration;
using Shared.Helpers.KeyValueStorage;
using Shared.Helpers.Queue;
using Shared.Helpers.Security;
using Shared.Helpers.Services;
using Shared.Helpers.Sms;
using Shared.Integration.ExternalSystems;
using Shared.Integration.Models;
using Swashbuckle.AspNetCore.Filters;
using ThreeDS;
using ThreeDS.Configuration;
using Transactions.Api.Controllers;
using Transactions.Api.Extensions;
using Transactions.Api.Models.Tokens;
using Transactions.Api.Services;
using Transactions.Business.Data;
using Transactions.Business.Services;
using Transactions.Shared;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;
using Upay;
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
            var apiConfig = Configuration.GetSection("API").Get<ApiSettings>();

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
                                            "https://ecng-merchants.azurewebsites.net",
                                            apiConfig.MerchantProfileURL,
                                            apiConfig.MerchantsManagementApiAddress
                                            )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithExposedHeaders("X-Version");
                    });
            });

            services.AddSignalR()
                .AddAzureSignalR(opts =>
                {
                    opts.ConnectionString = appConfig.AzureSignalRConnectionString;
                });

            services.AddDistributedMemoryCache();

            var upayConfig = Configuration.GetSection("UpayGlobalSettings").Get<UpayGlobalSettings>();
            var nayaxConfig = Configuration.GetSection("NayaxGlobalSettings").Get<Nayax.Configuration.NayaxGlobalSettings>();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.ClaimsIssuer = identity.Authority;
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
                                            new Claim(ClaimTypes.Role, Roles.Merchant),
                                            new Claim(ClaimTypes.Role, Roles.Manager)
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
                })
                .AddApiKeyInHeader(Auth.ApiKeyAuthenticationScheme, options =>
                {
                    options.SuppressWWWAuthenticateHeader = true;
                    options.KeyName = "API-key";
                    options.Events = new ApiKeyEvents
                    {
                        OnValidateKey = async (context) =>
                        {
                            if (upayConfig.ApiKey != null && upayConfig.ApiKey.Equals(context.ApiKey, StringComparison.OrdinalIgnoreCase))
                            {
                                var claims = new[]
                                {
                                    new Claim(ClaimTypes.Role, Roles.UPayAPI)
                                };
                                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                                context.Success();
                            }
                            else if (nayaxConfig.APIKey != null && nayaxConfig.APIKey.Equals(context.ApiKey, StringComparison.OrdinalIgnoreCase))
                            {
                                var claims = new[]
                                {
                                    new Claim(ClaimTypes.Role, Roles.NayaxAPI)
                                };
                                context.Principal = new ClaimsPrincipal(new ClaimsIdentity(claims, context.Scheme.Name));
                                context.Success();
                            }
                            else
                            {
                                context.NoResult();
                            }
                        },
                    };
                })
                .AddJwtBearer("SignalR", options =>
                {
                    options.ClaimsIssuer = identity.Authority;
                    options.Authority = identity.Authority;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateAudience = false,
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = async (context) =>
                        {
                            var path = context.HttpContext.Request.Path;

                            if (string.IsNullOrEmpty(context.Token) && path.StartsWithSegments("/hubs"))
                            {
                                // attempt to read the access token from the query string
                                var accessToken = context.Request.Query["access_token"];
                                string headerToken = context.Request.Headers["Authorization"];

                                if (!string.IsNullOrEmpty(accessToken))
                                {
                                    // Read the token out of the query string
                                    context.Token = accessToken;
                                }
                                else if (!string.IsNullOrEmpty(headerToken))
                                {
                                    context.Token = headerToken.Replace("Bearer ", string.Empty);
                                }
                            }
                        },
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
                        },
                    };
                });

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = false;  // TODO: remove for production

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.Terminal, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal()));
                options.AddPolicy(Policy.TerminalOrMerchantFrontendOrAdmin, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal() || context.User.IsMerchantFrontend() || context.User.IsAdmin()));
                options.AddPolicy(Policy.TerminalOrManagerOrAdmin, policy =>
                    policy.RequireAssertion(context => context.User.IsManager() || context.User.IsTerminal() || context.User.IsAdmin()));
                options.AddPolicy(Policy.MerchantFrontend, policy =>
                   policy.RequireAssertion(context => context.User.IsMerchantFrontend()));
                options.AddPolicy(Policy.AnyAdmin, policy =>
                   policy.RequireAssertion(context => context.User.IsAdmin()));
                options.AddPolicy(Policy.UPayAPI, policy =>
                   policy.RequireAssertion(context => context.User.IsUpayApi()));
                options.AddPolicy(Policy.NayaxAPI, policy =>
                   policy.RequireAssertion(context => context.User.IsNayaxApi()));
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

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath1 = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath1);

                var xmlPath2 = System.IO.Path.Combine(AppContext.BaseDirectory, "Transactions.Shared.xml");
                c.IncludeXmlComments(xmlPath2);

                var xmlPath3 = System.IO.Path.Combine(AppContext.BaseDirectory, "Shared.Integration.xml");
                c.IncludeXmlComments(xmlPath3);

                c.ExampleFilters();

                c.SchemaFilter<EnumSchemaFilter>();
                c.SchemaFilter<SwaggerExcludeFilter>();

                //Temporary fix for Can't use schemaId .. The same schemaId is already used for type. Exception
                //TODO: fix types and remove
                c.CustomSchemaIds(type =>
                {
                    if (type.IsGenericType)
                    {
                        return $"{type.Name}_{type.GetGenericArguments().FirstOrDefault().Name}";
                    }
                    else
                    {
                        return type.Name;
                    }
                });

                //c.DocumentFilter<PolymorphismDocumentFilter<Models.Transactions.CreateTransactionRequest>>();
                //c.SchemaFilter<PolymorphismSchemaFilter<Models.Transactions.CreateTransactionRequest>>();

                // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // add Security information to each operation for OAuth2
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                // if you're using the SecurityRequirementsOperationFilter, you also need to tell Swashbuckle you're using OAuth2
                //c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                //{
                //    Description = "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                //    In = ParameterLocation.Header,
                //    Name = "Authorization",
                //    Type = SecuritySchemeType.OAuth2
                //});

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "oauth2" }
                        },
                        new string[] { }
                    }
                });

                c.AddEnumsWithValuesFixFilters(services, o =>
                {
                    // add schema filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema
                    //o.ApplySchemaFilter = true;

                    // alias for replacing 'x-enumNames' in swagger document
                    //o.XEnumNamesAlias = "x-enum-varnames";

                    // alias for replacing 'x-enumDescriptions' in swagger document
                    //o.XEnumDescriptionsAlias = "x-enum-descriptions";

                    // add parameter filter to fix enums (add 'x-enumNames' for NSwag or its alias from XEnumNamesAlias) in schema parameters
                    //o.ApplyParameterFilter = true;

                    // add document filter to fix enums displaying in swagger document
                    // o.ApplyDocumentFilter = true;

                    // add descriptions from DescriptionAttribute or xml-comments to fix enums (add 'x-enumDescriptions' or its alias from XEnumDescriptionsAlias for schema extensions) for applied filters
                    o.IncludeDescriptions = true;

                    // add remarks for descriptions from xml-comments
                    o.IncludeXEnumRemarks = true;

                    // get descriptions from DescriptionAttribute then from xml-comments
                    o.DescriptionSource = DescriptionSources.DescriptionAttributesThenXmlComments;

                    // get descriptions from xml-file comments on the specified path
                    // should use "options.IncludeXmlComments(xmlFilePath);" before
                    o.IncludeXmlCommentsFrom(xmlPath1);
                    o.IncludeXmlCommentsFrom(xmlPath2);
                    o.IncludeXmlCommentsFrom(xmlPath3);
                });
            });

            // DI
            services.AddDbContext<Merchants.Business.Data.MerchantsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("MerchantsConnection")));
            services.AddScoped<IMerchantsService, MerchantsService>();
            services.AddScoped<ITerminalsService, TerminalsService>();
            services.AddScoped<IConsumersService, ConsumersService>();
            services.AddScoped<IItemsService, ItemsService>();
            services.AddScoped<IImpersonationService, ImpersonationService>();
            services.AddScoped<IShvaTerminalsService, ShvaTerminalService>();
            services.AddScoped<IPinPadDevicesService, PinPadDevicesService>();
            services.AddScoped<IMasavFileService, MasavFileService>();
            services.AddTransient<CardTokenController, CardTokenController>();
            services.AddTransient<InvoicingController, InvoicingController>();
            services.AddTransient<PaymentRequestsController, PaymentRequestsController>();
            services.AddTransient<BillingController, BillingController>();
            services.AddTransient<Controllers.External.BitApiController, Controllers.External.BitApiController>();

            services.AddSingleton<IExternalSystemsService, ExternalSystemService>(serviceProvider =>
            {
                return new ExternalSystemService(Path.Combine(AppContext.BaseDirectory, "external-systems.json"));
            });

            services.Configure<ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<AzureKeyVaultSettings>(Configuration.GetSection("AzureKeyVaultTokenStorageSettings"));
            services.Configure<ApiSettings>(Configuration.GetSection("API"));

            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            services.AddDbContext<TransactionsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ITransactionsService, TransactionsService>();
            services.AddScoped<ICreditCardTokenService, CreditCardTokenService>();
            services.AddScoped<INayaxTransactionsParametersService, NayaxTransactionsParametersService>();
            services.AddScoped<IBillingDealService, BillingDealService>();
            services.AddScoped<IFutureBillingsService, BillingDealService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentRequestsService, PaymentRequestsService>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddAutoMapper(typeof(Startup));

            services.AddSingleton<IKeyValueStorage<CreditCardTokenKeyVault>, AzureKeyValueStorage<CreditCardTokenKeyVault>>();

            // integration
            services.Configure<Shva.ShvaGlobalSettings>(Configuration.GetSection("ShvaGlobalSettings"));
            services.Configure<Nayax.Configuration.NayaxGlobalSettings>(Configuration.GetSection("NayaxGlobalSettings"));
            services.Configure<ClearingHouse.ClearingHouseGlobalSettings>(Configuration.GetSection("ClearingHouseGlobalSettings"));
            services.Configure<Upay.UpayGlobalSettings>(Configuration.GetSection("UpayGlobalSettings"));
            services.Configure<EasyInvoice.EasyInvoiceGlobalSettings>(Configuration.GetSection("EasyInvoiceGlobalSettings"));
            services.Configure<RapidOne.Configuration.RapidOneGlobalSettings>(Configuration.GetSection("RapidOneGlobalSettings"));
            services.Configure<Ecwid.Configuration.EcwidGlobalSettings>(Configuration.GetSection("EcwidGlobalSettings"));

            services.AddSingleton<IAggregatorResolver, AggregatorResolver>();
            services.AddSingleton<IProcessorResolver, ProcessorResolver>();
            services.AddSingleton<IInvoicingResolver, InvoicingResolver>();

            X509Certificate2 shvaCertificate = null;

            var shvaConfig = Configuration.GetSection("ShvaGlobalSettings").Get<Shva.ShvaGlobalSettings>();

            if (!string.IsNullOrWhiteSpace(shvaConfig.CertificateThumbprint))
            {
                try
                {
                    using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        certStore.Open(OpenFlags.ReadOnly);
                        X509Certificate2Collection certCollection = certStore.Certificates.Find(
                            X509FindType.FindByThumbprint,
                            shvaConfig.CertificateThumbprint,
                            false);
                        if (certCollection.Count > 0)
                        {
                            shvaCertificate = certCollection[0];
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Cannot load Shva certificate {shvaConfig.CertificateThumbprint}: {ex.Message}");
                }
            }

            services.AddSingleton<Shva.ShvaProcessor, Shva.ShvaProcessor>(serviceProvider =>
            {
                var shvaCfg = serviceProvider.GetRequiredService<IOptions<Shva.ShvaGlobalSettings>>();
                var webApiClient = new WebApiClient(shvaCertificate);
                var logger = serviceProvider.GetRequiredService<ILogger<Shva.ShvaProcessor>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.ShvaRequestsLogStorageTable, cfg.ShvaRequestsLogStorageTable);

                return new Shva.ShvaProcessor(webApiClient, shvaCfg, logger, storageService);
            });

            services.AddSingleton<Nayax.NayaxProcessor, Nayax.NayaxProcessor>(serviceProvider =>
            {
                var nayaxCfg = serviceProvider.GetRequiredService<IOptions<Nayax.Configuration.NayaxGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<Nayax.NayaxProcessor>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.NayaxRequestsLogStorageTable, cfg.NayaxRequestsLogStorageTable);

                return new Nayax.NayaxProcessor(webApiClient, nayaxCfg, logger, storageService);
            });

            services.AddSingleton<ClearingHouse.ClearingHouseAggregator, ClearingHouse.ClearingHouseAggregator>(serviceProvider =>
            {
                var chCfg = serviceProvider.GetRequiredService<IOptions<ClearingHouse.ClearingHouseGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<ClearingHouse.ClearingHouseAggregator>>();
                var mapper = serviceProvider.GetRequiredService<IMapper>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.ClearingHouseRequestsLogStorageTable, cfg.ClearingHouseRequestsLogStorageTable);
                var tokenSvc = new WebApiClientTokenService(webApiClient.HttpClient, chCfg);

                return new ClearingHouse.ClearingHouseAggregator(webApiClient, logger, chCfg, tokenSvc, storageService, mapper);
            });

            services.AddSingleton<Upay.UpayAggregator, Upay.UpayAggregator>(serviceProvider =>
            {
                var chCfg = serviceProvider.GetRequiredService<IOptions<Upay.UpayGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<Upay.UpayAggregator>>();
                var mapper = serviceProvider.GetRequiredService<IMapper>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.UpayRequestsLogStorageTable, cfg.UpayRequestsLogStorageTable);
                var tokenSvc = new WebApiClientTokenService(webApiClient.HttpClient, chCfg);

                return new Upay.UpayAggregator(webApiClient, logger, chCfg, tokenSvc, storageService, mapper);
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

            services.AddSingleton<RapidOne.RapidOneInvoicing, RapidOne.RapidOneInvoicing>(serviceProvider =>
            {
                var ecCfg = serviceProvider.GetRequiredService<IOptions<RapidOne.Configuration.RapidOneGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<RapidOne.RapidOneInvoicing>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.RapidInvoiceRequestsLogStorageTable, cfg.RapidInvoiceRequestsLogStorageTable);
                return new RapidOne.RapidOneInvoicing(webApiClient, ecCfg, logger, storageService);
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

                return new QueueResolver()
                    .AddQueue(cfg.InvoiceQueueName, new AzureQueue(cfg.DefaultStorageConnectionString, cfg.InvoiceQueueName)) // TODO: change cfg to constant in QueueResolver
                    .AddQueue(cfg.BillingDealsQueueName, new AzureQueue(cfg.DefaultStorageConnectionString, cfg.BillingDealsQueueName))
                    .AddQueue(QueueResolver.UpdateTerminalSHVAParametersQueue, new AzureQueue(cfg.DefaultStorageConnectionString, QueueResolver.UpdateTerminalSHVAParametersQueue))
                    .AddQueue(QueueResolver.TransmissionQueue, new AzureQueue(cfg.DefaultStorageConnectionString, QueueResolver.TransmissionQueue));
            });

            var appInsightsConfig = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsightsSettings>();

            services.AddSingleton<IMetricsService, MetricsService>(serviceProvider =>
            {
                return new MetricsService(appInsightsConfig);
            });

            services.AddSingleton<IPaymentIntentService, PaymentIntentService>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                return new PaymentIntentService(cfg.DefaultStorageConnectionString, cfg.PaymentIntentStorageTable);
            });

            services.AddSingleton<ISmsService, InforUMobileSmsService>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.SmsTableName, null);
                var inforUMobileSmsSettings = Configuration.GetSection("InforUMobileSmsSettings")?.Get<InforUMobileSmsSettings>();

                var webApiClient = new WebApiClient();

                var logger = serviceProvider.GetRequiredService<ILogger<InforUMobileSmsService>>();
                var metrics = serviceProvider.GetRequiredService<IMetricsService>();

                var doNotSendSms = cfg.DoNotSendSms;

                return new InforUMobileSmsService(webApiClient, inforUMobileSmsSettings, logger, storageService, metrics, doNotSendSms);
            });
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

            services.AddSingleton<BasicServices.Services.IExcelService, BasicServices.Services.ExcelService>(serviceProvider =>
            {
                var appCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var logger = serviceProvider.GetRequiredService<ILogger<BasicServices.Services.ExcelService>>();
                var blobStorageService = new BasicServices.BlobStorage.BlobStorageService(appCfg.PublicStorageConnectionString, "excel", logger);

                return new BasicServices.Services.ExcelService(blobStorageService);
            });

            // Bit integration

            X509Certificate2 bitCertificate = null;

            services.Configure<BitGlobalSettings>(Configuration.GetSection("BitGlobalSettings"));
            var bitConfig = Configuration.GetSection("BitGlobalSettings").Get<BitGlobalSettings>();

            try
            {
                using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    certStore.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certCollection = certStore.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        bitConfig.CertificateThumbprint,
                        false);
                    if (certCollection.Count > 0)
                    {
                        bitCertificate = certCollection[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cannot load bit certificate {bitConfig.CertificateThumbprint}: {ex.Message}");
            }

            services.AddSingleton<Bit.BitProcessor, Bit.BitProcessor>(serviceProvider =>
            {
                var webApiClient = new WebApiClient(bitCertificate);
                var logger = serviceProvider.GetRequiredService<ILogger<Bit.BitProcessor>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.BitRequestsLogStorageTable, cfg.BitRequestsLogStorageTable);

                var bitOptionsConfig = serviceProvider.GetRequiredService<IOptions<BitGlobalSettings>>();
                var tokenSvc = new BitTokensService(webApiClient, bitOptionsConfig.Value);

                return new Bit.BitProcessor(bitOptionsConfig, webApiClient, logger, storageService, tokenSvc);
            });

            services.AddSingleton<Ecwid.Api.IEcwidApiClient, Ecwid.Api.EcwidApiClient>(serviceProvider =>
            {
                var ecwidCfg = serviceProvider.GetRequiredService<IOptions<Ecwid.Configuration.EcwidGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<Ecwid.Api.EcwidApiClient>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.EcwidRequestsLogStorageTable, cfg.EcwidRequestsLogStorageTable);

                return new Ecwid.Api.EcwidApiClient(webApiClient, storageService, logger, ecwidCfg);
            });

            // 3DS integration
            services.Configure<ThreedDSGlobalConfiguration>(Configuration.GetSection("ThreedDSGlobalConfiguration"));

            services.AddSingleton<ThreeDSService, ThreeDSService>(serviceProvider =>
            {
                var threedsCfg = serviceProvider.GetRequiredService<IOptions<ThreedDSGlobalConfiguration>>();
                var webApiClient = new WebApiClient(shvaCertificate);
                var logger = serviceProvider.GetRequiredService<ILogger<ThreeDSService>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, "threeds", "threeds");

                return new ThreeDSService(threedsCfg, webApiClient, logger, storageService);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "TransactionsApi"));
            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();

            app.UseRequestResponseLogging();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseStaticFiles();

            app.UseHttpsRedirection();

            var apiSettings = serviceProvider.GetRequiredService<IOptions<ApiSettings>>().Value;

            if (apiSettings != null && !string.IsNullOrEmpty(apiSettings.Version))
            {
                app.Use(async (context, next) =>
                {
                    context.Response.Headers.Add(Headers.API_VERSION_HEADER, apiSettings.Version);
                    await next.Invoke();
                });
            }
            else
            {
                logger.LogError("Missing API.Version in appsettings.json");
            }

            app.UseRequestLocalization(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("he")
                };
                options.DefaultRequestCulture = new RequestCulture("he");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Transactions Api V1");
            //});

            app.UseReDoc(c =>
            {
                c.DocumentTitle = "EasyCard Transactions Api V1";
                c.SpecUrl = "/swagger/v1/swagger.json";
            });

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseAzureSignalR(endpoints =>
            {
                endpoints.MapHub<Hubs.TransactionsHub>("/hubs/transactions");
            });

            loggerFactory.CreateLogger("TransactionsApi.Startup").LogInformation("Started");
        }
    }
}
