using Merchants.Shared.Enums;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Checkout
{
    public class TerminalCheckoutCombinedSettings
    {
        [StringLength(250)]
        public string DefaultChargeDescription { get; set; }

        public string MarketingName { get; set; }

        public Guid? TerminalID { get; set; }

        public decimal? VATRate { get; set; }

        public string MerchantLogo { get; set; }

        // TODO: move to other place (sepends on implementation)
        public string CustomCssReference { get; set; }

        public bool? IssueInvoice { get; set; }

        public bool? AllowPinPad { get; set; }

        // TODO: validation
        public IEnumerable<string> RedirectUrls { get; set; }

        public int? MinCreditInstallments { get; set; }

        public int? MaxCreditInstallments { get; set; }

        public int? MinInstallments { get; set; }

        public int? MaxInstallments { get; set; }

        public bool LegacyRedirectResponse { get; set; }

        public IEnumerable<TransactionTypeEnum> TransactionTypes { get; set; }

        public IEnumerable<PinPadDevice> PinPadDevices { get; set; }

        public string BlobBaseAddress { get; set; }

        public bool? NationalIDRequired { get; set; }

        public bool? CvvRequired { get; set; }

        public ICollection<FeatureEnum> EnabledFeatures { get; set; }

        public bool? AllowBit { get; set; }

        public bool? EnableThreeDS { get; set; }

        /// <summary>
        /// If 3DSecure raises error - continue flow without 3ds
        /// </summary>
        public bool? ContinueInCaseOf3DSecureError { get; set; }
    }
}
