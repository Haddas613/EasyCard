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
    public class Terminal : IEntityBase<Guid>, IMerchantEntity
    {
        public Terminal()
        {
            Settings = new TerminalSettings();
            BillingSettings = new TerminalBillingSettings();
            InvoiceSettings = new TerminalInvoiceSettings();
            PaymentRequestSettings = new TerminalPaymentRequestSettings();
            CheckoutSettings = new TerminalCheckoutSettings();
            Integrations = new HashSet<TerminalExternalSystem>();
            EnabledFeatures = new HashSet<FeatureEnum>();
            Created = DateTime.UtcNow;
            TerminalID = Guid.NewGuid().GetSequentialGuid(Created.Value);
        }

        public Guid TerminalID { get; set; }

        public Guid MerchantID { get; set; }

        public Merchant.Merchant Merchant { get; set; }

        public string Label { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public TerminalStatusEnum Status { get; set; }

        public DateTime? ActivityStartDate { get; set; }

        public DateTime? Created { get; set; }

        public DateTime? Updated { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        public TerminalPaymentRequestSettings PaymentRequestSettings { get; set; }

        public TerminalCheckoutSettings CheckoutSettings { get; set; }

        public virtual ICollection<TerminalExternalSystem> Integrations { get; set; }

        public virtual ICollection<FeatureEnum> EnabledFeatures { get; set; }

        public byte[] SharedApiKey { get; set; }

        /// <summary>
        /// Copy of the corresponding <see cref="TerminalExternalSystem"/> processor integration settings terminal reference.
        /// For search purposes.
        /// </summary>
        public string ProcessorTerminalReference { get; set; }

        /// <summary>
        /// Copy of the corresponding <see cref="TerminalExternalSystem"/> aggregator integration settings terminal reference.
        /// For search purposes.
        /// </summary>
        public string AggregatorTerminalReference { get; set; }

        /// <summary>
        /// Copy of the corresponding <see cref="TerminalExternalSystem"/> pin pad integration settings terminal reference.
        /// For search purposes.
        /// </summary>
        public string PinPadProcessorTerminalReference { get; set; }

        public long? TerminalTemplateID { get; set; }

        public TerminalBankDetails BankDetails { get; set; }

        public Guid GetID()
        {
            return TerminalID;
        }
    }
}
