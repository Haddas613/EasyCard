using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.PaymentRequests.Enums;
using Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.PaymentRequests
{
    public class PaymentRequestResponse : PaymentRequestInfo
    {
        /// <summary>
        /// Terminal
        /// </summary>
        public Guid? TerminalID { get; set; }

        [EnumDataType(typeof(PaymentRequestStatusEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public PaymentRequestStatusEnum Status { get; set; }

        public Guid? ConsumerID { get; set; }

        public Guid? PaymentTransactionID { get; set; }

        public string PaymentRequestUrl { get; set; }
    }
}
