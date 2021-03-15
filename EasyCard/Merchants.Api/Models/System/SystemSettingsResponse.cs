using Merchants.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.System
{
    public class SystemSettingsResponse
    {
        public int SystemSettingsID { get; set; }

        public SystemGlobalSettings Settings { get; set; }

        public SystemBillingSettings BillingSettings { get; set; }

        public SystemInvoiceSettings InvoiceSettings { get; set; }

        public SystemPaymentRequestSettings PaymentRequestSettings { get; set; }

        public SystemCheckoutSettings CheckoutSettings { get; set; }

        public byte[] UpdateTimestamp { get; set; }
    }
}
