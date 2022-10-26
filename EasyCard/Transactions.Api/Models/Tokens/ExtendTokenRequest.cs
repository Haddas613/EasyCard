using Newtonsoft.Json;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using SharedHelpers = Shared.Helpers;

namespace Transactions.Api.Models.Tokens
{
    public class ExtendTokenRequest
    {
        [Required]
        public Guid? CreditCardTokenID { get; set; }

        [Required]
        [JsonConverter(typeof(CardExpirationConverter))]
        [SharedHelpers.Models.CardExpirationValidator]
        public virtual CardExpiration CardExpiration { get; set; }
    }
}
