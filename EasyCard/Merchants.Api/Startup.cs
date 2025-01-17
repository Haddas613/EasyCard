using AutoMapper;
using BasicServices;
using BasicServices.BlobStorage;
using Bit.Configuration;
using IdentityServer4.AccessTokenValidation;
using IdentityServerClient;
using Merchants.Api.Data;
using Merchants.Business.Data;
using Merchants.Business.Services;
using Merchants.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Nayax;
using Nayax.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Api.Logging;
using Shared.Api.Models.Binding;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Configuration;
using Shared.Helpers.Security;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using SharedApi = Shared.Api;

namespace Merchants.Api
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

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(
                            "http://localhost:8081",
                            "http://ecng-merchants.azurewebsites.net")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders(Headers.API_VERSION_HEADER)
                        .WithExposedHeaders(Headers.UI_VERSION_HEADER);
                    });
            });

            services.AddLogging(logging =>
            {
                logging.AddConfiguration(Configuration.GetSection("Logging"));
                logging.AddDebug();
                logging.AddAzureWebAppDiagnostics();
            });

            var identity = Configuration.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();

            services.AddDistributedMemoryCache();

            //TODO
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.ClaimsIssuer = identity.Authority;
                    options.Authority = identity.Authority;
                    options.RequireHttpsMetadata = true;
                    options.RoleClaimType = "role";
                    options.NameClaimType = "name";
                    options.EnableCaching = true;
                });

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = false; // TODO: remove for production

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    Policy.ManagementApi, policy =>
                     policy.RequireAssertion(context =>
                        context.User.IsManagementService()));

                options.AddPolicy(Policy.AnyAdmin, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsAdmin() || context.User.IsManagementService()));

                options.AddPolicy(Policy.OnlyBillingAdmin, policy =>
                    policy.RequireAssertion(context =>
                        context.User.IsBillingAdmin()));

                options.AddPolicy(Policy.ManagementApiOrBillingAdmin, policy =>
                     policy.RequireAssertion(context =>
                        context.User.IsBillingAdmin() || context.User.IsManagementService()));
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

            //Required for all infrastructure json serializers such as GlobalExceptionHandler to follow camelCase convention
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),

                //MissingMemberHandling = MissingMemberHandling.Error NOTE: do not enable it

                //Converters = new[] { new TrimmingJsonConverter() } NOTE: do not enable it
            };

            services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
            {
                // Disables [ApiController] automatic bad request result for invalid models
                options.SuppressModelStateInvalidFilter = true;
            });

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerExamplesFromAssemblies();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Merchants API",
                });

                c.ExampleFilters();

                //Temporary fix for Can't use schemaId .. The same schemaId is already used for type. Exception
                //TODO: fix types and remove
                c.CustomSchemaIds(type => type.ToString());

                c.SchemaFilter<SharedApi.Swagger.EnumSchemaFilter>();
                c.SchemaFilter<SharedApi.Swagger.SwaggerExcludeFilter>();

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
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

            // DI: basics
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<ApiSettings>(Configuration.GetSection("API"));
            services.Configure<ApplicationInsightsSettings>(Configuration.GetSection("ApplicationInsights"));
            services.Configure<ClearingHouse.ClearingHouseGlobalSettings>(Configuration.GetSection("ClearingHouseGlobalSettings"));
            services.Configure<EasyInvoice.EasyInvoiceGlobalSettings>(Configuration.GetSection("EasyInvoiceGlobalSettings"));
            services.Configure<RapidOne.Configuration.RapidOneGlobalSettings>(Configuration.GetSection("RapidOneGlobalSettings"));

            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            var egaeg = Configuration.GetConnectionString("DefaultConnection");

            // DI: services
            services.AddDbContext<MerchantsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IMerchantsService, MerchantsService>();
            services.AddScoped<ITerminalsService, TerminalsService>();
            services.AddScoped<ITerminalTemplatesService, TerminalTemplatesService>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddScoped<IFeaturesService, FeaturesService>();
            services.AddScoped<IPlansService, PlansService>();
            services.AddScoped<IImpersonationService, ImpersonationService>();
            services.AddScoped<IShvaTerminalsService, ShvaTerminalService>();
            services.AddScoped<IPinPadDevicesService, PinPadDevicesService>();
            services.AddAutoMapper(typeof(Startup));

            // DI: identity client

            services.Configure<IdentityServerClientSettings>(Configuration.GetSection("IdentityServerClient"));
            services.Configure<Shva.ShvaGlobalSettings>(Configuration.GetSection("ShvaGlobalSettings"));
            services.Configure<NayaxGlobalSettings>(Configuration.GetSection("NayaxGlobalSettings"));

            services.AddSingleton<IExternalSystemsService, ExternalSystemService>(serviceProvider =>
            {
                return new ExternalSystemService(Path.Combine(AppContext.BaseDirectory, "external-systems.json"));
            });

            services.AddSingleton<IUserManagementClient, UserManagementClient>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<UserManagementClient>>();
                var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

                return new UserManagementClient(webApiClient, logger, cfg, tokenService);
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

            services.AddSingleton<NayaxProcessor, NayaxProcessor>(serviceProvider =>
            {
                var nayaxCfg = serviceProvider.GetRequiredService<IOptions<NayaxGlobalSettings>>();
                var webApiClient = new WebApiClient(TimeSpan.FromMinutes(2));
                var logger = serviceProvider.GetRequiredService<ILogger<NayaxProcessor>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.NayaxRequestsLogStorageTable, cfg.NayaxRequestsLogStorageTable);

                return new NayaxProcessor(webApiClient, nayaxCfg, logger, storageService);
            });

            services.AddSingleton<IBlobStorageService, BlobStorageService>(serviceProvider =>
            {
                var appCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var logger = serviceProvider.GetRequiredService<ILogger<BlobStorageService>>();
                var blobStorageService = new BlobStorageService(appCfg.PublicStorageConnectionString, appCfg.PublicBlobStorageTable, logger);

                return blobStorageService;
            });

            //NOTE: this is not fully functioning Bit Processor as we only need it for request logs
            services.AddSingleton<Bit.BitProcessor, Bit.BitProcessor>(serviceProvider =>
            {
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<Bit.BitProcessor>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.BitRequestsLogStorageTable, cfg.BitRequestsLogStorageTable);

                var bitOptionsConfig = serviceProvider.GetRequiredService<IOptions<BitGlobalSettings>>();
                //var tokenSvc = new BitTokensService(webApiClient, bitOptionsConfig.Value);

                return new Bit.BitProcessor(bitOptionsConfig, webApiClient, logger, storageService, null);
            });

            // DI: request logging

            services.Configure<RequestResponseLoggingSettings>((options) =>
            {
                options.RequestsLogStorageTable = appConfig.RequestsLogStorageTable;
                options.StorageConnectionString = appConfig.DefaultStorageConnectionString;
            });

            services.AddSingleton<IRequestLogStorageService, RequestLogStorageService>();

            // system log

            services.AddSingleton<IDatabaseLogService, DatabaseLogService>(serviceProvider =>
            {
                return new DatabaseLogService(Configuration.GetConnectionString("SystemConnection"));
            });

            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.EncrKeyForSharedApiKey);
            });
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "AdminApi"));
            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();

            app.UseRequestResponseLogging();

            app.UseMiddleware<ExceptionMiddleware>();

            var apiSettings = Configuration.GetSection("API")?.Get<ApiSettings>();

            if (apiSettings != null && !string.IsNullOrEmpty(apiSettings.Version))
            {
                app.Use(async (context, next) =>
                {
                    context.Response.Headers.Add(Headers.API_VERSION_HEADER, apiSettings.Version);
                    context.Response.Headers.Add(Headers.UI_VERSION_HEADER, apiSettings.Version);
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
            app.UseStaticFiles();

            //app.UseHttpsRedirection();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Merchants API V1");

                //c.RoutePrefix = string.Empty;
            });

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                endpoints.MapFallbackToController("Index", "Home");
            });

            //app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
            //{
            //    builder.UseSpa(spa =>
            //    {
            //        spa.Options.DefaultPage = "/";
            //    });
            //});

            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");

            try
            {
                SeedData.EnsureSeedData(connectionString);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Failed to Seed data {ex.Message}");
            }

            loggerFactory.CreateLogger("AdminApi.Startup").LogInformation("Started");
        }
    }
}
