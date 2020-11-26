using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shared.Helpers;
using Shared.Helpers.Security;
using Transactions.Api.Client;

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

            services.AddControllersWithViews()
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
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            services.Configure<Models.ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<TransactionsApiClientConfig>(Configuration.GetSection("ApiConfig"));
            

            services.Configure<IdentityServerClientSettings>(Configuration.GetSection("IdentityServerClient"));

            services.AddSingleton<ITransactionsApiClient, TransactionsApiClient>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                var apiCfg = serviceProvider.GetRequiredService<IOptions<TransactionsApiClientConfig>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<TransactionsApiClient>>();
                var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

                return new TransactionsApiClient(webApiClient, logger, tokenService, apiCfg);
            });

            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<Models.ApplicationSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.SecretKey);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

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
