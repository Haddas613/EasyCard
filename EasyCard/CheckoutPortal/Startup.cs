using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shared.Api.Configuration;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Security;
using Transactions.Api.Client;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using SharedApi = Shared.Api;
using CheckoutPortal.Services;
using CheckoutPortal.Models;
using Ecwid.Configuration;
using Merchants.Business.Services;
using ThreeDS;
using ThreeDS.Configuration;
using Merchants.Business.Data;
using BasicServices;
using Merchants.Api.Client;
using System.Security.Cryptography.X509Certificates;
using System.Diagnostics;

namespace CheckoutPortal
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
                logging.AddConsole();
                logging.AddDebug();
                logging.AddAzureWebAppDiagnostics();
            });

            services.AddAntiforgery(options =>
            {
                options.SuppressXFrameOptionsHeader = true;
                options.Cookie.SameSite = SameSiteMode.None;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });

            services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.Converters.Add(new StringEnumConverter());
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.DefaultValueHandling = DefaultValueHandling.Include;

                    // Note: do not use options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore; - use [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)] attribute in place
                })
                .AddRazorRuntimeCompilation();

            services.AddSignalR()
                .AddAzureSignalR(opts =>
                {
                    opts.ConnectionString = appConfig.AzureSignalRConnectionString;
                });

            //Required for all infrastructure json serializers such as GlobalExceptionHandler to follow camelCase convention
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            services.Configure<Models.ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<ApiSettings>(Configuration.GetSection("API")); 
            services.Configure<EcwidGlobalSettings>(Configuration.GetSection("EcwidGlobalSettings"));
            services.Configure<ThreedDSGlobalConfiguration>(Configuration.GetSection("ThreedDSGlobalConfiguration"));
            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            services.AddAutoMapper(typeof(Startup));

            services.Configure<IdentityServerClientSettings>(Configuration.GetSection("IdentityServerClient"));

            services.AddSingleton<IWebApiClient, WebApiClient>();
            services.AddSingleton<IWebApiClientTokenService, WebApiClientTokenService>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                var client = serviceProvider.GetRequiredService<IWebApiClient>();
                return new WebApiClientTokenService(client.HttpClient, cfg);
            });
            //services.AddScoped<ITerminalsService, TerminalsService>();
            services.AddScoped<IMerchantsApiClient, MerchantsApiClient>(serviceProvider =>
            {
                var apiCfg = serviceProvider.GetRequiredService<IOptions<ApiSettings>>();
                //var logger = serviceProvider.GetRequiredService<ILogger<MerchantsApiClient>>();

                var webApiClient = serviceProvider.GetRequiredService<IWebApiClient>();
                var tokenService = serviceProvider.GetRequiredService<IWebApiClientTokenService>();

                var context = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var cultureFeature = context.HttpContext.Features.Get<IRequestCultureFeature>();

                var merchantsApiClient = new MerchantsApiClient(webApiClient, /*logger,*/ tokenService, apiCfg);

                merchantsApiClient.Headers.Add("Accept-Language", cultureFeature.RequestCulture.Culture.Name);

                return merchantsApiClient;
            });

            services.AddScoped<ITransactionsApiClient, TransactionsApiClient>((serviceProvider) =>
            {
                var apiCfg = serviceProvider.GetRequiredService<IOptions<ApiSettings>>();
                var logger = serviceProvider.GetRequiredService<ILogger<TransactionsApiClient>>();
                var webApiClient = serviceProvider.GetRequiredService<IWebApiClient>();
                var tokenService = serviceProvider.GetRequiredService<IWebApiClientTokenService>();

                var context = serviceProvider.GetRequiredService<IHttpContextAccessor>();
                var cultureFeature = context.HttpContext.Features.Get<IRequestCultureFeature>();

                var transactionApiClient = new TransactionsApiClient(webApiClient, /*logger,*/ tokenService, apiCfg);
                transactionApiClient.Headers.Add("Accept-Language", cultureFeature.RequestCulture.Culture.Name);

                return transactionApiClient;
            });

            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<Models.ApplicationSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.EncrKeyForSharedApiKey);
            });

            services.AddSingleton<IApiClientsFactory, ApiClientsFactory>(serviceProvider =>
            {
                var apiCfg = serviceProvider.GetRequiredService<IOptions<ApiSettings>>();
                return new ApiClientsFactory(apiCfg);
            }); 

            services.AddSingleton<ITerminalApiKeyTokenServiceFactory, TerminalApiKeyTokenServiceFactory>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                return new TerminalApiKeyTokenServiceFactory(cfg);
            });

            services.AddDbContext<MerchantsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ITerminalsService, TerminalsService>();

            // Bit integration

            X509Certificate2 threesDSCertificate = null;

            services.Configure<ThreedDSGlobalConfiguration>(Configuration.GetSection("ThreedDSGlobalConfiguration"));
            var threedDSGlobalConfig = Configuration.GetSection("ThreedDSGlobalConfiguration").Get<ThreedDSGlobalConfiguration>();

            try
            {
                using (X509Store certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
                {
                    certStore.Open(OpenFlags.ReadOnly);
                    X509Certificate2Collection certCollection = certStore.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        threedDSGlobalConfig.CertificateThumbprint,
                        false);
                    if (certCollection.Count > 0)
                    {
                        threesDSCertificate = certCollection[0];
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cannot load ThreeDS certificate {threedDSGlobalConfig.CertificateThumbprint}: {ex.Message}");
            }

            services.AddSingleton<ThreeDSService, ThreeDSService>(serviceProvider =>
            {
                //var threedsCfg = services.Configure<ThreedDSGlobalConfiguration>(Configuration.GetSection("ThreedDSGlobalConfiguration")); //serviceProvider.GetRequiredService<ThreedDSGlobalConfiguration>();
                var threedsCfg = serviceProvider.GetRequiredService<IOptions<ThreedDSGlobalConfiguration>>();
                var webApiClient = new WebApiClient(threesDSCertificate);
                var logger = serviceProvider.GetRequiredService<ILogger<ThreeDSService>>();
                // var storageService = new IntegrationRequestLogStorageService(threedsCfg.DefaultStorageConnectionString, cfg.ShvaRequestsLogStorageTable, cfg.ShvaRequestsLogStorageTable);

                /*serviceProvider.GetRequiredService<IOptions<Shva.ShvaGlobalSettings>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                 */
                return new ThreeDSService(threedsCfg, webApiClient, logger/*, storageService*/);
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

            // TODO: Temporary solution
            services.AddSingleton<MemoryPaymentRequestStorage>();

            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "Checkout"));
            var logger = serviceProvider.GetRequiredService<ILogger<Startup>>();

            app.UseExceptionHandler("/Home/Error");
            
            app.UseHsts();

            app.UseXXssProtection(options => options.EnabledWithBlockMode());
            //app.UseXfo(options => options.SameOrigin());
            app.UseReferrerPolicy(opts => opts.NoReferrerWhenDowngrade());

            app.UseCsp(options => options
                .DefaultSources(s => s.Self()
                    .CustomSources("data:", "https:", "wss:"))
                .StyleSources(s => s.Self()
                    .CustomSources("ecngpublic.blob.core.windows.net")
                )
                .ScriptSources(s => s.Self()
                    .CustomSources("az416426.vo.msecnd.net", "public.bankhapoalim.co.il", "d35z3p2poghz10.cloudfront.net")
                )
                //.FrameAncestors(s => s.Self())
                //.FormActions(s => s.Self())
            );

            app.UseHttpsRedirection();

            

            app.UseStaticFiles();

            app.UseRouting();

            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>().Value;
            app.UseRequestLocalization(localizationOptions);

            app.UseAuthorization();

            app.UseAzureSignalR(endpoints =>
            {
                endpoints.MapHub<Hubs.TransactionsHub>("/hubs/transactions");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
