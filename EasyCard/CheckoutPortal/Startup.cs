using System;
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
                .AddRazorRuntimeCompilation();

            //Required for all infrastructure json serializers such as GlobalExceptionHandler to follow camelCase convention
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            services.Configure<Models.ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<ApiSettings>(Configuration.GetSection("API"));

            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            services.AddAutoMapper(typeof(Startup));

            services.Configure<IdentityServerClientSettings>(Configuration.GetSection("IdentityServerClient"));

            services.AddSingleton<ITransactionsApiClient, TransactionsApiClient>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                var apiCfg = serviceProvider.GetRequiredService<IOptions<ApiSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<TransactionsApiClient>>();
                var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

                return new TransactionsApiClient(webApiClient, /*logger,*/ tokenService, apiCfg);
            });

            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<Models.ApplicationSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.EncrKeyForSharedApiKey);
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
                options.RequestCultureProviders = new List<IRequestCultureProvider> { new QueryStringRequestCultureProvider() };
            });

            services.AddMvc().AddViewLocalization();
            services.AddSingleton<CommonLocalizationService>();

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
                    .CustomSources("data:")
                    .CustomSources("https:"))
                .StyleSources(s => s.Self()
                    .CustomSources("ecngpublic.blob.core.windows.net")
                )
                .ScriptSources(s => s.Self()
                    .CustomSources("az416426.vo.msecnd.net")
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

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
