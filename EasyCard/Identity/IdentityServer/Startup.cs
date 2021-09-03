using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using AutoMapper;
using BasicServices;
using IdentityServer.Data;
using IdentityServer.Helpers;
using IdentityServer.Models;
using IdentityServer.Models.Configuration;
using IdentityServer.Security;
using IdentityServer.Security.Auditing;
using IdentityServer.Services;
using IdentityServer4;
using IdentityServer4.Extensions;
using IdentityServer4.Validation;
using InforU;
using Merchants.Api.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
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
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Configuration;
using Shared.Helpers.Email;
using Shared.Helpers.Security;
using Shared.Helpers.Services;
using Shared.Helpers.Sms;
using Shared.Integration;
using SharedApi = Shared.Api;

namespace IdentityServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;

            this.environment = environment;
        }

        public IConfiguration Configuration { get; }

        private IWebHostEnvironment environment;

        private ApiSettings apiConfig;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = Configuration.GetSection("AppConfig")?.Get<ApplicationSettings>();
            var identity = Configuration.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();
            var azureADConfig = Configuration.GetSection("AzureADConfig")?.Get<AzureADSettings>();
            apiConfig = Configuration.GetSection("API")?.Get<ApiSettings>();

            services.AddLogging(logging =>
            {
                logging.AddConfiguration(Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
                logging.AddAzureWebAppDiagnostics();
            });

            services.AddAntiforgery(options =>
            {
                options.SuppressXFrameOptionsHeader = true;
            });

            services.AddControllersWithViews()
                 .AddNewtonsoftJson(options =>
                 {
                     options.SerializerSettings.Converters.Add(new StringEnumConverter());
                     options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                     options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;

                     // Note: do not use options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; - use [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)] attribute in place
                 })
                 .AddRazorRuntimeCompilation(); //TODO: remove on release?

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
            });

            services.Configure<SecuritySettings>(options =>
            {
                options.PasswordExpirationDays = 90;
                options.RememberLastPasswords = 4;
            });

            //Required for all infrastructure json serializers such as GlobalExceptionHandler to follow camelCase convention
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Identity Server",
                });

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            services.Configure<ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<ApiSettings>(Configuration.GetSection("API"));
            services.Configure<CryptoSettings>(Configuration.GetSection("CryptoConfig"));

            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            services.AddAutoMapper(typeof(Startup));

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); // TODO: replace

            var clientsConfig = new Config(apiConfig, identity);

            var builder = services.AddIdentityServer(options =>
            {
                options.IssuerUri = identity.Authority;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddInMemoryIdentityResources(clientsConfig.Ids)
            .AddInMemoryApiResources(clientsConfig.Apis)
            .AddInMemoryApiScopes(clientsConfig.ApiScopes)
            .AddInMemoryClients(clientsConfig.Clients)
            .AddAspNetIdentity<ApplicationUser>()

            //.AddProfileService<ProfileService>()
            .AddExtensionGrantValidator<DelegationGrantValidator>();

            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();

            // TODO: not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddIdentityServerAuthentication("token", options =>
                {
                    options.Authority = identity.Authority;
                    options.RequireHttpsMetadata = true;
                    options.EnableCaching = true;
                })
                .AddOpenIdConnect("oidc", "Azure AD", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.Authority = azureADConfig.AzureAdAuthority;
                    options.ClientId = azureADConfig.AzureAdClientId;
                    options.ResponseType = "id_token";
                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateAudience = false,
                    };
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    Policy.ManagementApi,
                    policy => policy.RequireClaim("scope", "management_api"));
            });

            X509Certificate2 cert = null;

            try
            {
                if (environment.IsDevelopment())
                {
                    cert = new X509Certificate2(Convert.FromBase64String(config.InternalCertificate), "idsrv");
                }
                else
                {
                    using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                    {
                        certStore.Open(OpenFlags.ReadOnly);
                        X509Certificate2Collection certCollection = certStore.Certificates.Find(
                            X509FindType.FindByThumbprint,
                            config.InternalCertificate,
                            false);
                        if (certCollection.Count > 0)
                        {
                            cert = certCollection[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cannot load certificate {config.InternalCertificate}: {ex.Message}");
            }

            services.AddSingleton<IEmailSender, AzureQueueEmailSender>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                var queue = new AzureQueue(cfg.DefaultStorageConnectionString, cfg.EmailQueueName);
                return new AzureQueueEmailSender(queue, cfg.DefaultStorageConnectionString, cfg.EmailTableName);
            });

            services.AddSingleton<ICryptoService>(new CryptoService(cert));
            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<CryptoSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.SecretKey);
            });

            services.AddScoped<IAuditLogger, AuditLogger>();
            services.AddScoped<UserManageService, UserManageService>();
            services.AddScoped<UserHelpers, UserHelpers>();
            services.AddScoped<UserSecurityService, UserSecurityService>();

            // DI: request logging

            services.Configure<RequestResponseLoggingSettings>((options) =>
            {
                options.RequestsLogStorageTable = config.RequestsLogStorageTable;
                options.StorageConnectionString = config.DefaultStorageConnectionString;
            });

            var appInsightsConfig = Configuration.GetSection("ApplicationInsights").Get<ApplicationInsightsSettings>();

            services.AddSingleton<IMetricsService, MetricsService>(serviceProvider =>
            {
                return new MetricsService(appInsightsConfig);
            });

            services.AddSingleton<ISmsService, InforUMobileSmsService>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.SmsTableName, null);
                var inforUMobileSmsSettings = Configuration.GetSection("InforUMobileSmsSettings")?.Get<InforUMobileSmsSettings>();

                var webApiClient = new WebApiClient();

                var logger = serviceProvider.GetRequiredService<ILogger<InforUMobileSmsService>>();
                var metrics = serviceProvider.GetRequiredService<IMetricsService>();

                var doNotSendSms = cfg.TwoFactorAuthenticationDoNotSendSms;

                return new InforUMobileSmsService(webApiClient, inforUMobileSmsSettings, logger, storageService, metrics, doNotSendSms);
            });

            services.AddSingleton<IRequestLogStorageService, RequestLogStorageService>();

            services.Configure<ApiSettings>(Configuration.GetSection("ApiConfig"));
            services.Configure<IdentityServerClientSettings>(Configuration.GetSection("IdentityServerClient"));
            services.Configure<AzureADSettings>(Configuration.GetSection("AzureADConfig"));

            services.AddSingleton<IWebApiClient, WebApiClient>();
            services.AddSingleton<IWebApiClientTokenService, WebApiClientTokenService>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                var client = serviceProvider.GetRequiredService<IWebApiClient>();
                return new WebApiClientTokenService(client.HttpClient, cfg);
            });

            services.AddScoped<IMerchantsApiClient, MerchantsApiClient>(serviceProvider =>
            {
                var apiCfg = serviceProvider.GetRequiredService<IOptions<ApiSettings>>();
                var logger = serviceProvider.GetRequiredService<ILogger<MerchantsApiClient>>();

                var webApiClient = serviceProvider.GetRequiredService<IWebApiClient>();
                var tokenService = serviceProvider.GetRequiredService<IWebApiClientTokenService>();

                var context = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var cultureFeature = context.HttpContext.Features.Get<IRequestCultureFeature>();

                var merchantsApiClient = new MerchantsApiClient(webApiClient, logger, tokenService, apiCfg);

                merchantsApiClient.Headers.Add("Accept-Language", cultureFeature.RequestCulture.Culture.Name);

                return merchantsApiClient;
            });

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                 {
                    new CultureInfo("en"),
                    new CultureInfo("he"),
                 };
                options.DefaultRequestCulture = new RequestCulture("he");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders = new List<IRequestCultureProvider> { new CookieRequestCultureProvider() };
            });

            services.AddMvc().AddViewLocalization();
            services.AddSingleton<CommonLocalizationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "Identity"));

            app.UseRequestResponseLogging();

            app.UseExceptionHandler(GlobalExceptionHandler.HandleException);

            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();

            //app.UseCookiePolicy();

            app.UseXXssProtection(options => options.EnabledWithBlockMode());

            //app.UseXfo(options => options.SameOrigin());

            app.UseReferrerPolicy(opts => opts.NoReferrerWhenDowngrade());

            app.UseCsp(options => options
                .DefaultSources(s => s.Self()
                    .CustomSources("data:")
                    .CustomSources("https:"))
                .StyleSources(s => s.Self()
                    .CustomSources("ecngpublic.blob.core.windows.net")
                )
                .ScriptSources(s => s.Self()
                    .CustomSources("az416426.vo.msecnd.net")
                )
                .FrameAncestors(s => s.Self())
                .FormActions(s => s.Self()
                    .CustomSources(apiConfig.MerchantProfileURL, apiConfig.MerchantsManagementApiAddress, "http://localhost:8080/", "http://localhost:8081/", "login.microsoftonline.com")
                )
            );

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>().Value;

            app.Use((context, next) =>
            {
                context.SetIdentityServerOrigin(cfg.Authority);
                return next();
            });

            app.UseRouting();

            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(localizationOptions);

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });

            Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleManager = serviceProvider.GetService<Microsoft.AspNetCore.Identity.RoleManager<IdentityRole>>();

            try
            {
                foreach (var role in new[] { Roles.Merchant, Roles.BusinessAdministrator, Roles.BillingAdministrator, Roles.Manager })
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }

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

            loggerFactory.CreateLogger("IdentityServer.Startup").LogInformation("Started");
        }
    }
}
