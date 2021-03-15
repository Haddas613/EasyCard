using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.User;
using Merchants.Shared.Enums;
using Merchants.Shared.Models;
using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Terminal
{
    public class TerminalTemplate : IEntityBase<long>
    {
        public TerminalTemplate()
        {
            Settings = new TerminalSettings();
            BillingSettings = new TerminalBillingSettings();
            InvoiceSettings = new TerminalInvoiceSettings();
            PaymentRequestSettings = new TerminalPaymentRequestSettings();
            CheckoutSettings = new TerminalCheckoutSettings();
            Integrations = new HashSet<TerminalTemplateExternalSystem>();
            EnabledFeatures = new HashSet<FeatureEnum>();
            Created = DateTime.UtcNow;
        }

        public long TerminalTemplateID { get; set; }

        public string Label { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public DateTime? Created { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        public TerminalPaymentRequestSettings PaymentRequestSettings { get; set; }

        public TerminalCheckoutSettings CheckoutSettings { get; set; }

        public virtual ICollection<TerminalTemplateExternalSystem> Integrations { get; set; }

        public virtual ICollection<FeatureEnum> EnabledFeatures { get; set; }

        public bool Active { get; set; }

        public long GetID()
        {
            return TerminalTemplateID;
        }
    }
}
