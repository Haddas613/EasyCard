using Newtonsoft.Json;
using Shared.Api.Models.Binding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class UpdateTerminalRequest
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(50, MinimumLength = 6)]
        [JsonConverter(typeof(TrimmingJsonConverter))]
        public string Label { get; set; }

        [Required]
        public TerminalSettingsUpdate Settings { get; set; }

        [Required]
        public TerminalBillingSettingsUpdate BillingSettings { get; set; }

        [Required]
        public TerminalInvoiceSettingsUpdate InvoiceSettings { get; set; }

        [Required]
        public TerminalPaymentRequestSettingsUpdate PaymentRequestSettings { get; set; }

        [Required]
        public TerminalCheckoutSettingsUpdate CheckoutSettings { get; set; }
    }
}
