using Merchants.Shared.Models;
using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Merchants.Api.Models.Terminal
{
    public class TerminalRequest
    {
        [Required]
        public Guid MerchantID { get; set; }

        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 6)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string Label { get; set; }

        [Required]
        public TerminalSettings Settings { get; set; }

        [Required]
        public TerminalBillingSettings BillingSettings { get; set; }

        [Required]
        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        [Required]
        public TerminalPaymentRequestSettings PaymentRequestSettings { get; set; }

        [Required]
        public TerminalCheckoutSettings CheckoutSettings { get; set; }
    }
}
