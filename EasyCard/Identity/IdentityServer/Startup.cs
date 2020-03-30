using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using BasicServices;
using IdentityServer.Data;
using IdentityServer.Models;
using IdentityServer.Security;
using IdentityServer.Security.Auditing;
using IdentityServer.Services;
using IdentityServer4;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
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
using Shared.Helpers.Email;
using Shared.Helpers.Security;

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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var config = Configuration.GetSection("AppConfig")?.Get<ApplicationSettings>();
            var identity = Configuration.GetSection("IdentityServerClient")?.Get<IdentityServerClientSettings>();

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

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders(); // TODO: replace

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
            .AddInMemoryIdentityResources(Config.Ids)
            .AddInMemoryApiResources(Config.Apis)
            .AddInMemoryClients(Config.Clients)
            .AddAspNetIdentity<ApplicationUser>()
            .AddProfileService<ProfileService>()
            .AddExtensionGrantValidator<DelegationGrantValidator>();

            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidator>();

            // TODO: not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddIdentityServerAuthentication("token", options =>
                {
                    options.Authority = identity.Authority;
                    options.RequireHttpsMetadata = true;
                    options.ApiName = "management_api";
                    options.EnableCaching = true;
                });

                //.AddOpenIdConnect("oidc", "OpenID Connect", options =>
                //{
                //    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                //    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                //    options.Authority = identity.AzureAdAuthority;
                //    options.ClientId = identity.AzureAdClientId;
                //    options.ResponseType = "id_token";
                //    options.SaveTokens = true;
                //    options.GetClaimsFromUserInfoEndpoint = true;

                //    options.TokenValidationParameters = new TokenValidationParameters
                //    {
                //        ValidateAudience = false,
                //    };
                //});

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

            services.AddSingleton<IEmailSender, EventHubEmailSender>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                return new EventHubEmailSender(cfg.EmailEventHubConnectionString, cfg.EmailEventHubName);
            });

            services.AddSingleton<ICryptoService>(new CryptoService(cert));

            services.AddScoped<IAuditLogger, AuditLogger>();
            services.AddScoped<ITerminalApiKeyService, TerminalApiKeyService>();

            // DI: request logging

            services.Configure<RequestResponseLoggingSettings>((options) =>
            {
                options.RequestsLogStorageTable = config.RequestsLogStorageTable;
                options.StorageConnectionString = config.DefaultStorageConnectionString;
            });

            services.AddSingleton<IRequestLogStorageService, RequestLogStorageService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory, IServiceProvider serviceProvider)
        {
            app.UseRequestResponseLogging();

            app.UseExceptionHandler(GlobalExceptionHandler.HandleException);

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

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });

            Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleManager = serviceProvider.GetService<Microsoft.AspNetCore.Identity.RoleManager<IdentityRole>>();

            try
            {
                foreach (var role in new[] { "Merchant", "BusinessAdministrator", "BillingAdministrator" })
                {
                    if (!roleManager.RoleExistsAsync(role).Result)
                    {
                        roleManager.CreateAsync(new IdentityRole(role)).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            var config = serviceProvider.GetRequiredService<IConfiguration>();
            var connectionString = config.GetConnectionString("DefaultConnection");

            try
            {
                SeedData.EnsureSeedData(connectionString);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
