using Merchants.Api.Models.Merchant;
using Merchants.Api.Models.User;
using Merchants.Shared.Enums;
using Merchants.Shared.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalResponse
    {
        public TerminalResponse()
        {
            Settings = new TerminalSettings();
            BillingSettings = new TerminalBillingSettings();
            InvoiceSettings = new TerminalInvoiceSettings();
            PaymentRequestSettings = new TerminalPaymentRequestSettings();
            CheckoutSettings = new TerminalCheckoutSettings();
            /*BankDetails = new TerminalBankDetails();*/
        }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public string Label { get; set; }

        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }

        public MerchantSummary Merchant { get; set; }

        public IEnumerable<UserSummary> Users { get; set; }

        public IEnumerable<TerminalExternalSystemDetails> Integrations { get; set; }

        public IEnumerable<FeatureEnum> EnabledFeatures { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        public TerminalPaymentRequestSettings PaymentRequestSettings { get; set; }

        public TerminalCheckoutSettings CheckoutSettings { get; set; }

        public TerminalBankDetails BankDetails { get; set; }

        public byte[] SharedApiKey { get; set; }
    }
}
