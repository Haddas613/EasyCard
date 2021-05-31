using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Api.UI;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions.Enums;
using Transactions.Shared.Enums;
using Enums = Transactions.Shared.Enums;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.Transactions
{
    /// <summary>
    /// Payment transaction details
    /// </summary>
    public class TransactionResponseAdmin : TransactionResponse
    {
        /// <summary>
        /// Merchant name
        /// </summary>
        public string MerchantName { get; set; }
    }
}
