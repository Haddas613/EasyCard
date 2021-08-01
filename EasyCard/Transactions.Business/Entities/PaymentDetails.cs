using JsonSubTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

namespace Transactions.Business.Entities
{
    [JsonConverter(typeof(JsonSubtypes), "PaymentType")]
    [JsonSubtypes.KnownSubType(typeof(ChequeDetails), PaymentTypeEnum.Cheque)]
    [JsonSubtypes.KnownSubType(typeof(CashDetails), PaymentTypeEnum.Cash)]
    [JsonSubtypes.KnownSubType(typeof(CreditCardPaymentDetails), PaymentTypeEnum.Card)]
    public class PaymentDetails
    {
        public virtual PaymentTypeEnum PaymentType { get; set; }
    }
}
