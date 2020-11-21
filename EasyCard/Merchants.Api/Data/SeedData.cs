using Merchants.Business.Data;
using Merchants.Business.Services;
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
                    var systemSettingsService = scope.ServiceProvider.GetService<ISystemSettingsService>();

                    // System settings
                    var systemSettings = systemSettingsService.GetSystemSettings().Result;
                    if (systemSettings == null)
                    {
                        systemSettings = new Business.Entities.System.SystemSettings
                        {
                            BillingSettings = new Shared.Models.TerminalBillingSettings
                            {
                                CreateRecurrentPaymentsAutomatically = false,
                            },
                            CheckoutSettings = new Shared.Models.TerminalCheckoutSettings
                            {
                            },
                            InvoiceSettings = new Shared.Models.TerminalInvoiceSettings
                            {
                                DefaultInvoiceSubject = "Invoice from ECNG",
                                DefaultInvoiceType = SharedIntegration.Models.Invoicing.InvoiceTypeEnum.InvoiceWithPaymentInfo
                            },
                            PaymentRequestSettings = new Shared.Models.TerminalPaymentRequestSettings
                            {
                                DefaultRequestSubject = "Payment request from ECNG",
                                FromAddress = "no-reply@ecng.co.il"
                            },
                            Settings = new Shared.Models.TerminalSettings
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
                                VATRate = 0.17m
                            }
                        };

                        systemSettingsService.UpdateSystemSettings(systemSettings).Wait();
                    }
                }
            }
        }
    }
}
