using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Events
{
    public class CustomEvent : CustomEventBase
    {
        public CustomEvent()
        {
            EventID = Guid.NewGuid().GetSequentialGuid(DateTime.UtcNow);
        }

        public const string TransactionCreated = "TransactionCreated";

        public const string TransactionRejected = "TransactionRejected";

        public const string CardTokenCreated = "CardTokenCreated";

        public const string CardTokenDeleted = "CardTokenDeleted";

        public const string CreditCardExpired = "CreditCardExpired";

        public const string BillingDealCreated = "BillingDealCreated";

        public const string BillingDealUpdated = "BillingDealUpdated";

        public const string InvoiceGenerated = "InvoiceGenerated";

        public const string InvoiceGenerationFailed = "InvoiceGenerationFailed";

        public const string ConsumerCreated = "ConsumerCreated";

        public const string ConsumerUpdated = "ConsumerUpdated";

        public const string InvoiceCanceled = "InvoiceCanceled";

        public const string InvoiceCancellationFailed = "InvoiceCancellationFailed";

        [JsonIgnore]
        public object Entity { get; set; }
    }
}
