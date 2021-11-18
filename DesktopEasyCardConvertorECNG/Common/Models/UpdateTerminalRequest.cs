using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    public class UpdateTerminalRequest
    {
        public string Label { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        public TerminalPaymentRequestSettingsUpdate PaymentRequestSettings { get; set; }

        public TerminalCheckoutSettings CheckoutSettings { get; set; }

        //public IEnumerable<FeatureEnum> EnabledFeatures { get; set; }

        public TerminalBankDetails BankDetails { get; set; }
    }
}

