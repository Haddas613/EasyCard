using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration
{
    public class Metrics
    {
        public const string SmsSent = "SmsSent";
        public const string SmsError = "SmsError";

        public const string TransactionCreated = "TransactionCreated";
        public const string TransactionRejected = "TransactionRejected";

        public const string BillingDealCreated = "BillingDealCreated";
        public const string BillingDealUpdated = "BillingDealUpdated";
        public const string BillingError = "BillingError";
        public const string BillingCardExpired = "BillingCardExpired";

        public const string CardTokenCreated = "CardTokenCreated";
        public const string CardTokenRejected = "CardTokenRejected";

        public const string ConsumerCreated = "ConsumerCreated";
        public const string ConsumerUpdated = "ConsumerUpdated";
    }
}
