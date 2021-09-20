using Shared.Api.Models;
using Shared.Api.Swagger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reporting.Api.Models.Tokens
{
    public class TokensTransactionsFilter : FilterBase
    {
        public Guid? CreditCardTokenID { get; set; }

        public string CardNumber { get; set; }

        public string CardOwnerNationalID { get; set; }

        public string CardOwnerName { get; set; }

        public Guid? TerminalID { get; set; }

        [SwaggerExclude]
        public Guid? MerchantID { get; set; }

        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}
