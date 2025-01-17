using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using IdentityServer4.AccessTokenValidation;
using Merchants.Business.Data;
using Merchants.Business.Services;
using Merchants.Shared;
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
using Reporting.Business.Services;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Configuration;
using Shared.Helpers.Security;
using Shared.Helpers.Services;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Business.Data;
using Transactions.Business.Services;
using SharedApi = Shared.Api;

namespace Reporting.Api
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
            });

            var appConfig = Configuration.GetSection("AppConfig").Get<ApplicationSettings>();
            var apiConfig = Configuration.GetSection("API").Get<ApiSettings>();

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
                                            apiConfig.MerchantsManagementApiAddress)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders(Headers.API_VERSION_HEADER);
                    });
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

                    options.JwtBearerEvents = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnTokenValidated = async (context) =>
                        {
                            try
                            {
                                var svc = context.HttpContext.RequestServices.GetService<IImpersonationService>();
                                await svc.SetImpersonationClaims(context.Principal);
                            }
                            catch (Exception ex)
                            {
                                // TODO: logging
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                            }
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.Terminal, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal()));
                options.AddPolicy(Policy.TerminalOrMerchantFrontend, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal() || context.User.IsMerchantFrontend()));
                options.AddPolicy(Policy.TerminalOrManagerOrAdmin, policy =>
                    policy.RequireAssertion(context => context.User.IsManager() || context.User.IsTerminal() || context.User.IsAdmin()));
                options.AddPolicy(Policy.MerchantFrontend, policy =>
                   policy.RequireAssertion(context => context.User.IsMerchantFrontend()));
                options.AddPolicy(Policy.Admin, policy =>
                   policy.RequireAssertion(context => context.User.IsAdmin()));
            });

            //Required for all infrastructure json serializers such as GlobalExceptionHandler to follow camelCase convention
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

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
            services.AddSwaggerExamplesFromAssemblies();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "MerchantProfile API",
                });

                c.ExampleFilters();

                c.SchemaFilter<SharedApi.Swagger.EnumSchemaFilter>();
                c.SchemaFilter<SharedApi.Swagger.SwaggerExcludeFilter>();

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            // DI: basics
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppConfig"));

            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            services.AddDbContext<MerchantsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("MerchantsConnection")));
            services.AddScoped<IMerchantsService, MerchantsService>();
            services.AddScoped<ITerminalsService, TerminalsService>();
            services.AddScoped<IConsumersService, ConsumersService>();
            services.AddScoped<IItemsService, ItemsService>();
            services.AddScoped<ICurrencyRateService, CurrencyRateService>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddScoped<IImpersonationService, ImpersonationService>();

            services.AddDbContext<TransactionsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("TransactionsConnection")));
            services.AddScoped<ITransactionsService, TransactionsService>();
            services.AddScoped<ICreditCardTokenService, CreditCardTokenService>();

            services.AddScoped<IReportingService, ReportingService>();

            services.AddAutoMapper(typeof(Startup));

            // DI: request logging

            services.Configure<RequestResponseLoggingSettings>((options) =>
            {
                options.RequestsLogStorageTable = appConfig.RequestsLogStorageTable;
                options.StorageConnectionString = appConfig.DefaultStorageConnectionString;
            });

            services.AddSingleton<IRequestLogStorageService, RequestLogStorageService>();

            // DI: identity client

            services.Configure<IdentityServerClientSettings>(Configuration.GetSection("IdentityServerClient"));

            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.EncrKeyForSharedApiKey);
            });

            services.AddScoped<IDashboardService, DashboardService>(serviceProvider =>
            {
                var httpContext = serviceProvider.GetRequiredService<IHttpContextAccessorWrapper>();

                return new DashboardService(Configuration.GetConnectionString("DefaultConnection"), httpContext);
            });

            services.AddScoped<IAdminService, AdminService>(serviceProvider =>
            {
                var appInsightReaderService = serviceProvider.GetRequiredService<IAppInsightReaderService>();

                return new AdminService(Configuration.GetConnectionString("DefaultConnection"), Configuration.GetConnectionString("TransactionsConnection"), appInsightReaderService);
            });

            var appInsightsConfig = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsightsSettings>();

            services.AddSingleton<IAppInsightReaderService, AppInsightReaderService>(serviceProvider =>
            {
                var webApiClient = new WebApiClient(TimeSpan.FromSeconds(30));
                return new AppInsightReaderService(appInsightsConfig, webApiClient);
            });

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = false;  // TODO: remove for production
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "Reports"));
            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();

            app.UseRequestResponseLogging();

            app.UseMiddleware<ExceptionMiddleware>();

            app.UseStaticFiles();

            var apiSettings = Configuration.GetSection("API")?.Get<ApiSettings>();

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
                    new CultureInfo("en-IL"),
                    new CultureInfo("he-IL")
                };
                options.DefaultRequestCulture = new RequestCulture("en-IL");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reporting API V1");

                //c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            loggerFactory.CreateLogger("ReportingApi.Startup").LogInformation("Started");
        }
    }
}
