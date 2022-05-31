using JsonSubTypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.PaymentDetails
{
    [JsonConverter(typeof(JsonSubtypes), "PaymentType")]
    [JsonSubtypes.KnownSubType(typeof(ChequeDetails), PaymentTypeEnum.Cheque)]
    [JsonSubtypes.KnownSubType(typeof(CashDetails), PaymentTypeEnum.Cash)]
    [JsonSubtypes.KnownSubType(typeof(CreditCardPaymentDetails), PaymentTypeEnum.Card)]
    [JsonSubtypes.KnownSubType(typeof(BankTransferDetails), PaymentTypeEnum.Bank)]
    public class PaymentDetails
    {
        public virtual PaymentTypeEnum PaymentType { get; set; }

        public virtual decimal Amount { get; set; }
    }
}
