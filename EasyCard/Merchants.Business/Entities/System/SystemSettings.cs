using Merchants.Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.System
{
    public class SystemSettings
    {
        public SystemSettings()
        {
            Settings = new TerminalSettings();
            BillingSettings = new TerminalBillingSettings();
            InvoiceSettings = new TerminalInvoiceSettings();
            PaymentRequestSettings = new TerminalPaymentRequestSettings();
            CheckoutSettings = new TerminalCheckoutSettings();
        }

        public int SystemSettingsID { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        public TerminalPaymentRequestSettings PaymentRequestSettings { get; set; }

        public TerminalCheckoutSettings CheckoutSettings { get; set; }

        public byte[] UpdateTimestamp { get; set; }
    }
}
