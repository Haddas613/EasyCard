using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Transactions.Shared.Enums
{
    /// <summary>
    /// Generic transaction type
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TransactionTypeEnum
    {
        /// <summary>
        /// Simple deal type
        /// </summary>
        [EnumMember(Value= "regularDeal")]
        RegularDeal = 0,

        /// <summary>
        /// Deal to pay by parts
        /// </summary>
        [EnumMember(Value = "installments")]
        Installments = 1,

        /// <summary>
        /// Credit deal
        /// </summary>
        [EnumMember(Value = "credit")]
        Credit = 2,
    }
}
