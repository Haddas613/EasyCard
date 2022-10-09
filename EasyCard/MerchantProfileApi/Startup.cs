using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using BasicServices;
using BasicServices.BlobStorage;
using IdentityServer4.AccessTokenValidation;
using IdentityServerClient;
using Markdig;
using Markdig.Extensions.AutoIdentifiers;
using MerchantProfileApi.Extensions;
using Merchants.Business.Data;
using Merchants.Business.Services;
using Merchants.Shared;
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
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shared.Api;
using Shared.Api.Configuration;
using Shared.Api.Validation;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Configuration;
using Shared.Helpers.Security;
using Swashbuckle.AspNetCore.Filters;
using Transactions.Api.Client;
using Unchase.Swashbuckle.AspNetCore.Extensions.Extensions;
using Unchase.Swashbuckle.AspNetCore.Extensions.Options;
using Westwind.AspNetCore.Markdown;
using SharedApi = Shared.Api;

namespace ProfileApi
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
                logging.AddAzureWebAppDiagnostics();
            });

            services.AddAntiforgery(options =>
            {
                options.SuppressXFrameOptionsHeader = true;
            });

            var appConfig = Configuration.GetSection("AppConfig").Get<ApplicationSettings>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder.WithOrigins(
                            "http://localhost:4200",
                            "http://localhost:8080")
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithExposedHeaders(Headers.API_VERSION_HEADER)
                        .WithExposedHeaders(Headers.UI_VERSION_HEADER);
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

                    options.JwtBearerEvents = new JwtBearerEvents
                    {
                        OnChallenge = async (context) =>
                        {
                            //var path = context.HttpContext.Request.Path;
                            //if (path.StartsWithSegments("/hubs"))
                            //{
                            //    context.HandleResponse();
                            //}
                        },
                        OnMessageReceived = async (context) =>
                        {
                            if (string.IsNullOrEmpty(context.Token))
                            {
                                // attempt to read the access token from the query string
                                var accessToken = context.Request.Query["access_token"];

                                // If the request is for our hub...
                                var path = context.HttpContext.Request.Path;
                                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                                {
                                    // Read the token out of the query string
                                    context.Token = accessToken;
                                }
                            }
                        },
                        OnTokenValidated = async (context) =>
                        {
                            try
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
                            catch (Exception ex)
                            {
                                // TODO: logging
                                System.Diagnostics.Debug.WriteLine(ex.Message);
                            }
                        }
                    };
                });

            //services.TryAddEnumerable(
            //    ServiceDescriptor.Singleton<IPostConfigureOptions<JwtBearerOptions>, QueryJwtBearerOptions>());

            services.AddAuthorization(options =>
            {
                options.AddPolicy(Policy.Terminal, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal()));
                options.AddPolicy(Policy.TerminalOrMerchantFrontend, policy =>
                    policy.RequireAssertion(context => context.User.IsTerminal() || context.User.IsMerchantFrontend()));
                options.AddPolicy(Policy.MerchantFrontend, policy =>
                   policy.RequireAssertion(context => context.User.IsMerchantFrontend()));
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
                    Title = "EasyCard Metadata API",
                });

                c.ExampleFilters();

                c.SchemaFilter<Shared.Api.Swagger.EnumSchemaFilter>();
                c.SchemaFilter<SharedApi.Swagger.SwaggerExcludeFilter>();

                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.CustomSchemaIds(type =>
                {
                    if (type.IsGenericType)
                    {
                        return $"{type.Name.Replace("`1", string.Empty)}_{type.GetGenericArguments().FirstOrDefault().Name}";
                    }
                    else
                    {
                        return type.Name;
                    }
                });

                // Adds "(Auth)" to the summary so that you can see which endpoints have Authorization
                //c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();

                // add Security information to each operation for OAuth2
                c.OperationFilter<SecurityRequirementsOperationFilter>();

                // if you're using the SecurityRequirementsOperationFilter, you also need to tell Swashbuckle you're using OAuth2
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
                    o.IncludeXmlCommentsFrom(xmlPath);
                    //o.IncludeXmlCommentsFrom(xmlPath2);
                    //o.IncludeXmlCommentsFrom(xmlPath3);
                });
            });
            services.AddSwaggerGenNewtonsoftSupport();

            // DI: basics
            services.Configure<ApplicationSettings>(Configuration.GetSection("AppConfig"));
            services.Configure<ApiSettings>(Configuration.GetSection("API"));
            services.Configure<ApplicationInsightsSettings>(Configuration.GetSection("ApplicationInsights"));
            services.Configure<UISettings>(Configuration.GetSection("UI"));
            services.Configure<EasyInvoice.EasyInvoiceGlobalSettings>(Configuration.GetSection("EasyInvoiceGlobalSettings"));
            services.Configure<RapidOne.Configuration.RapidOneGlobalSettings>(Configuration.GetSection("RapidOneGlobalSettings"));

            services.AddHttpContextAccessor();

            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();

            services.AddDbContext<MerchantsContext>(opts => opts.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IMerchantsService, MerchantsService>();
            services.AddScoped<ITerminalsService, TerminalsService>();
            services.AddScoped<IConsumersService, ConsumersService>();
            services.AddScoped<IItemsService, ItemsService>();
            services.AddScoped<ICurrencyRateService, CurrencyRateService>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddScoped<IImpersonationService, ImpersonationService>();
            services.AddScoped<IFeaturesService, FeaturesService>(); 
            services.AddScoped<IMerchantConsentService, MerchantConsentService>();
            services.AddSingleton<IBlobStorageService, BlobStorageService>(serviceProvider =>
            {
                var appCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var logger = serviceProvider.GetRequiredService<ILogger<BlobStorageService>>();
                var blobStorageService = new BlobStorageService(appCfg.PublicStorageConnectionString, appCfg.PublicBlobStorageTable, logger);

                return blobStorageService;
            });

            services.AddSingleton<IExternalSystemsService, ExternalSystemService>(serviceProvider =>
            {
                return new ExternalSystemService(Path.Combine(AppContext.BaseDirectory, "external-systems.json"));
            });

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

            services.AddSingleton<ITransactionsApiClient, TransactionsApiClient>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                var apiCfg = serviceProvider.GetRequiredService<IOptions<ApiSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<TransactionsApiClient>>();
                var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

                return new TransactionsApiClient(webApiClient, /*logger,*/ tokenService, apiCfg);
            });

            services.AddSingleton<IUserManagementClient, UserManagementClient>(serviceProvider =>
            {
                var cfg = serviceProvider.GetRequiredService<IOptions<IdentityServerClientSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<UserManagementClient>>();
                var tokenService = new WebApiClientTokenService(webApiClient.HttpClient, cfg);

                return new UserManagementClient(webApiClient, logger, cfg, tokenService);
            });

            services.AddSingleton<EasyInvoice.ECInvoiceInvoicing, EasyInvoice.ECInvoiceInvoicing>(serviceProvider =>
            {
                var ecCfg = serviceProvider.GetRequiredService<IOptions<EasyInvoice.EasyInvoiceGlobalSettings>>();
                var webApiClient = new WebApiClient();
                var logger = serviceProvider.GetRequiredService<ILogger<EasyInvoice.ECInvoiceInvoicing>>();
                var cfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var storageService = new IntegrationRequestLogStorageService(cfg.DefaultStorageConnectionString, cfg.EasyInvoiceRequestsLogStorageTable, cfg.EasyInvoiceRequestsLogStorageTable);

                return new EasyInvoice.ECInvoiceInvoicing(webApiClient, ecCfg, logger, storageService);
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

            services.AddSingleton<ICryptoServiceCompact, AesGcmCryptoServiceCompact>(serviceProvider =>
            {
                var cryptoCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>()?.Value;
                return new AesGcmCryptoServiceCompact(cryptoCfg.EncrKeyForSharedApiKey);
            });

            services.AddSingleton<BasicServices.Services.IExcelService, BasicServices.Services.ExcelService>(serviceProvider =>
            {
                var appCfg = serviceProvider.GetRequiredService<IOptions<ApplicationSettings>>().Value;
                var logger = serviceProvider.GetRequiredService<ILogger<BasicServices.Services.ExcelService>>();
                var blobStorageService = new BasicServices.BlobStorage.BlobStorageService(appCfg.PublicStorageConnectionString, "excel", logger);

                return new BasicServices.Services.ExcelService(blobStorageService);
            });

            Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = false;  // TODO: remove for production
            services.AddApplicationInsightsTelemetry(Configuration["APPINSIGHTS_CONNECTIONSTRING"]);

            services.AddMarkdown(config =>
            {
                // optional Tag BlackList
                config.HtmlTagBlackList = "script|iframe|object|embed|form"; // default

                // Simplest: Use all default settings
                var folderConfig = config.AddMarkdownProcessingFolder("/doc/", "~/Views/__MarkdownPageTemplate.cshtml");

                // Customized Configuration: Set FolderConfiguration options
                //folderConfig = config.AddMarkdownProcessingFolder("/posts/", "~/Pages/__MarkdownPageTemplate.cshtml");

                //var folderConfig = config.AddMarkdownProcessingFolder("/doc/");

                // Optionally strip script/iframe/form/object/embed tags ++
                folderConfig.SanitizeHtml = false;  //  default

                // Optional configuration settings
                folderConfig.ProcessExtensionlessUrls = true;  // default
                folderConfig.ProcessMdFiles = true; // default

                // Optional pre-processing - with filled model
                folderConfig.PreProcess = (model, controller) =>
                {
                    // controller.ViewBag.Model = new MyCustomModel();
                };

                // folderConfig.BasePath = "https://github.com/RickStrahl/Westwind.AspNetCore.Markdow/raw/master";

                // Create your own IMarkdownParserFactory and IMarkdownParser implementation
                // to replace the default Markdown Processing
                //config.MarkdownParserFactory = new CustomMarkdownParserFactory();                 

                // optional custom MarkdigPipeline (using MarkDig; for extension methods)
                config.ConfigureMarkdigPipeline = builder =>
                {
                    builder.UseEmphasisExtras(Markdig.Extensions.EmphasisExtras.EmphasisExtraOptions.Default)
                        .UsePipeTables()
                        .UseGridTables()
                        .UseAutoIdentifiers(AutoIdentifierOptions.GitHub) // Headers get id="name"
                        .UseAutoLinks() // URLs are parsed into anchors
                        .UseAbbreviations()
                        .UseYamlFrontMatter()
                        .UseEmojiAndSmiley(true)
                        .UseListExtras()
                        .UseFigures()
                        .UseTaskLists()
                        .UseCustomContainers()
                        //.DisableHtml()   // renders HTML tags as text including script
                        .UseGenericAttributes();
                };
            });

            // We need to use MVC so we can use a Razor Configuration Template
            // for the Markdown Processing Middleware
            services.AddMvc()
                // have to let MVC know we have a controller otherwise it won't be found
                .AddApplicationPart(typeof(MarkdownPageProcessorMiddleware).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddProvider(new SharedApi.Logging.LoggerDatabaseProvider(Configuration.GetConnectionString("SystemConnection"), serviceProvider.GetService<IHttpContextAccessor>(), "Profile"));
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

            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            //app.UseSwaggerUI(c =>
            //{
            //    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MerchantProfile API V1");

            //    //c.RoutePrefix = string.Empty;
            //});

            app.UseReDoc(c =>
            {
                c.DocumentTitle = "EasyCard Transactions Api V1";
                c.SpecUrl = "/swagger/v1/swagger.json";
            });

            //app.UseXXssProtection(options => options.EnabledWithBlockMode());
            //app.UseXfo(options => options.SameOrigin());
            //app.UseReferrerPolicy(opts => opts.NoReferrerWhenDowngrade());

            //// TODO: enable CSP
            //app.UseCsp(options => options
            //    .DefaultSources(s => s.Self()
            //        .CustomSources("data:")
            //        .CustomSources("https:")
            //        )
            //    .StyleSources(s => s.Self()
            //        .CustomSources("ecngpublic.blob.core.windows.net", "fonts.googleapis.com", "use.fontawesome.com")
            //        .UnsafeInline()
            //    )
            //    .ScriptSources(s => s.Self()
            //        .CustomSources("az416426.vo.msecnd.net", "widget.intercom.io", "js.intercomcdn.com")
            //        .UnsafeEval()
            //    )
            //    .FrameAncestors(s => s.Self())
            //    .FormActions(s => s.Self())
            //);

            app.UseHttpsRedirection();

            app.UseMarkdown();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "wwwroot";
            });

            loggerFactory.CreateLogger("MerchantProfile.Startup").LogInformation("Started");
        }
    }
}
