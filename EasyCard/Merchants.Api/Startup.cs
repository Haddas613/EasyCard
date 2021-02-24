using AutoMapper;
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
using Shared.Helpers.Security;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
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
                        .WithExposedHeaders("X-Version");
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

            //services.AddAuthentication() // TODO: bearer
            //    .AddIdentityServerAuthentication("token", options =>
            //    {
            //        options.Authority = identity.Authority;
            //        options.RequireHttpsMetadata = true;
            //        options.RoleClaimType = "role";
            //        options.NameClaimType = "name";
            //        options.ApiName = "management_api"; // TODO
            //        options.EnableCaching = true;
            //    });

            //TODO
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = identity.Authority;
                    options.RequireHttpsMetadata = true;
                    options.RoleClaimType = "role";
                    options.NameClaimType = "name";
                    options.EnableCaching = true;
                });

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true; // TODO: remove for production

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

                c.SchemaFilter<SharedApi.Swagger.EnumSchemaFilter>();

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            // DI: basics
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppConfig"));

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

            services.AddAutoMapper(typeof(Startup));

            // DI: identity client

            services.Configure<IdentityServerClientSettings>(Configuration.GetSection("IdentityServerClient"));

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "AdminApi"));
            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();

            app.UseRequestResponseLogging();

            app.UseExceptionHandler(GlobalExceptionHandler.HandleException);

            var apiSettings = Configuration.GetSection("API")?.Get<ApiSettings>();

            if (apiSettings != null && !string.IsNullOrEmpty(apiSettings.Version))
            {
                app.Use(async (context, next) =>
                {
                    context.Response.Headers.Add("X-Version", apiSettings.Version);
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
                endpoints.MapControllers();
            });

            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Home}/{action=Index}/{id?}");
            //});

            app.MapWhen(x => !x.Request.Path.Value.StartsWith("/api"), builder =>
            {
                builder.UseSpa(spa =>
                {
                    spa.Options.SourcePath = "wwwroot";
                });
            });

            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");

            try
            {
                SeedData.EnsureSeedData(connectionString);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }

            loggerFactory.CreateLogger("AdminApi.Startup").LogInformation("Started");
        }
    }
}
