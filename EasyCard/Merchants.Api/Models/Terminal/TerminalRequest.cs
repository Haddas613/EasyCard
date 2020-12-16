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

        public Guid? TerminalTemplateID { get; set; }

        public TerminalSettings Settings { get; set; }

        public TerminalBillingSettings BillingSettings { get; set; }

        public TerminalInvoiceSettings InvoiceSettings { get; set; }

        public TerminalPaymentRequestSettings PaymentRequestSettings { get; set; }

        public TerminalCheckoutSettings CheckoutSettings { get; set; }
    }
}
