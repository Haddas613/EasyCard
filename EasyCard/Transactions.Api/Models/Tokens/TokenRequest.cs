using Newtonsoft.Json;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Tokens
{
    /// <summary>
    /// Store credit card details to make deals with this card in further deals
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class TokenRequest : CreditCardSecureDetails
    {
        /// <summary>
        /// EasyCard terminal reference
        /// </summary>
        [Required(AllowEmptyStrings = false)]
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Consumer ID
        /// </summary>
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        [MaxLength(50)]
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// Authorization code
        /// </summary>
        public string OKNumber { get; set; }
    }
}
