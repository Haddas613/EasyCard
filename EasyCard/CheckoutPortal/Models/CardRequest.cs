using Newtonsoft.Json;
using Shared.Helpers;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Shared.Helpers.Models;

namespace CheckoutPortal.Models
{
    public class CardRequest
    {
        /// <summary>
        /// Deal description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Consumer name
        /// </summary>
        public string Name { get; set; }

        [JsonConverter(typeof(TrimmingJsonConverter))]
        [IsraelNationalIDValidator]
        public string NationalID { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public CurrencyEnum Currency { get; set; }

        public decimal? Amount { get; set; }

        // TODO
        public bool UserAmount { get; set; }

        public string RedirectUrl { get; set; }

        public string ApiKey { get; set; }

        /// <summary>
        /// Payment request ID
        /// </summary>
        public string PaymentRequest { get; set; }

        /// <summary>
        /// Payment intent ID
        /// </summary>
        public string PaymentIntent { get; set; }

        public bool? IssueInvoice { get; set; }

        public bool? AllowPinPad { get; set; }

        public string Language { get; set; }
    }
}
