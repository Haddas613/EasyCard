using Merchants.Business.Data;
using Merchants.Business.Services;
using Merchants.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SharedIntegration = Shared.Integration;

namespace Merchants.Api.Data
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString)
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddHttpContextAccessor();
            services.AddScoped<IHttpContextAccessorWrapper, HttpContextAccessorWrapper>();
            services.AddScoped<ISystemSettingsService, SystemSettingsService>();
            services.AddDbContext<MerchantsContext>(opts => opts.UseSqlServer(connectionString));

            using (var serviceProvider = services.BuildServiceProvider())
            {
                using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    var context = scope.ServiceProvider.GetService<MerchantsContext>();

                    // System settings
                    var systemSettings = context.SystemSettings.AsNoTracking().FirstOrDefault();
                    if (systemSettings == null)
                    {
                        systemSettings = new Business.Entities.System.SystemSettings
                        {
                            BillingSettings = new Shared.Models.SystemBillingSettings
                            {
                                CreateRecurrentPaymentsAutomatically = false,
                            },
                            CheckoutSettings = new Shared.Models.SystemCheckoutSettings
                            {
                            },
                            InvoiceSettings = new Shared.Models.SystemInvoiceSettings
                            {
                                DefaultInvoiceSubject = "Invoice from ECNG",
                                DefaultInvoiceType = SharedIntegration.Models.Invoicing.InvoiceTypeEnum.InvoiceWithPaymentInfo
                            },
                            PaymentRequestSettings = new Shared.Models.SystemPaymentRequestSettings
                            {
                                DefaultRequestSubject = "Payment request from ECNG",
                                FromAddress = "no-reply@ecng.co.il"
                            },
                            Settings = new Shared.Models.SystemGlobalSettings
                            {
                                CvvRequired = true,
                                DefaultChargeDescription = "Goods and services from ECNG",
                                DefaultItemName = "Custom Item",
                                DefaultRefundDescription = "Refund from ECNG",
                                EnableCancellationOfUntransmittedTransactions = true,
                                J2Allowed = true,
                                J5Allowed = true,
                                MaxCreditInstallments = 12,
                                MaxInstallments = 12,
                                MinCreditInstallments = 2,
                                MinInstallments = 2,
                                NationalIDRequired = true,
                                VATRate = 0.17m,
                                DefaultSKU = "_"
                            }
                        };

                        context.SystemSettings.Add(systemSettings);
                        context.SaveChangesAsync().Wait();
                    }

                    SeedFeaturesData(context);
                }
            }
        }

        private static void SeedFeaturesData(MerchantsContext context)
        {
            var dbFeatures = context.Features.ToList().ToDictionary(k => k.FeatureID, v => v);
            var enumFeatures = Enum.GetValues(typeof(FeatureEnum)).Cast<FeatureEnum>();

            var addingFeatures = new List<Business.Entities.Merchant.Feature>();

            foreach (var @enum in enumFeatures)
            {
                if (!dbFeatures.ContainsKey(@enum))
                {
                    var name = Enum.GetName(typeof(FeatureEnum), @enum);
                    addingFeatures.Add(new Business.Entities.Merchant.Feature
                    {
                        FeatureID = @enum,
                        NameEN = name,
                        NameHE = name,
                        Price = 0
                    });
                }
            }

            if (addingFeatures.Count == 0)
            {
                return;
            }

            context.AddRange(addingFeatures);
            context.SaveChanges();
        }
    }
}
