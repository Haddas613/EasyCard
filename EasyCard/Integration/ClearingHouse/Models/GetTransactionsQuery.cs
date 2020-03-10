using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClearingHouse.Models
{
    public class GetTransactionsQuery
    {
        public int? PaymentGatewayID { get; set; }

        public int? Take { get; set; }

        public int? Skip { get; set; }

        public List<int> Status { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public long? MerchantID { get; set; }

        public string MerchantReference { get; set; }

        public List<int> RiskRate { get; set; }

        public string BusinessArea { get; set; }

        public string DealReference { get; set; }

        public string TerminalReference { get; set; }

        public string DealDescription { get; set; }

        public string ConsumerEmail { get; set; }

        public string ConsumerPhone { get; set; }

        public string ConsumerName { get; set; }

        public string Solek { get; set; }

        public string CreditCardVendor { get; set; }

        /// <summary>
        /// Total amount
        /// </summary>
        public double? PaymentAmountFrom { get; set; }

        public double? PaymentAmountTo { get; set; }

        public List<int> TransactionType { get; set; }

        public string Currency { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        public string MerchantName { get; set; }

        /// <summary>
        /// ?
        /// </summary>
        //[EnumDataType(typeof(TransactionTypeEnum))]
        [JsonConverter(typeof(StringEnumConverter))]
        public List<KYCStatusEnum> KycApprovalStatus { get; set; }
    }
}
