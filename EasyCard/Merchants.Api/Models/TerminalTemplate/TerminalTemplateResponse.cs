using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.Terminal;
using Merchants.Api.Models.User;
using Merchants.Shared.Enums;
using Merchants.Shared.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.TerminalTemplate
{
    public class TerminalTemplateResponse
    {
        public TerminalTemplateResponse()
        {
            Settings = new TerminalSettings();
            BillingSettings = new TerminalBillingSettings();
            InvoiceSettings = new TerminalInvoiceSettings();
            PaymentRequestSettings = new TerminalPaymentRequestSettings();
            CheckoutSettings = new TerminalCheckoutSettings();
        }

        public long TerminalTemplateID { get; set; }

        public string Label { get; set; }

        public bool Active { get; set; }

        public DateTime? Created { get; set; }

        public IEnumerable<TerminalExternalSystemDetails> Integrations { get; set; }

        public IEnumerable<FeatureEnum> EnabledFeatures { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        public TerminalPaymentRequestSettings PaymentRequestSettings { get; set; }

        public TerminalCheckoutSettings CheckoutSettings { get; set; }
    }
}
