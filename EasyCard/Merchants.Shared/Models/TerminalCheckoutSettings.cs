using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Shared.Models
{
    /// <summary>
    /// Settings for checkout (redirect) page
    /// </summary>
    public class TerminalCheckoutSettings
    {
        // TODO: move to other place (sepends on implementation)
        public string CustomCssReference { get; set; }

        public bool? IssueInvoice { get; set; }

        // TODO: validation
        public IEnumerable<string> RedirectUrls { get; set; }

        public bool LegacyRedirectResponse { get; set; }

        /// <summary>
        /// If 3DSecure raises error - continue flow without 3ds
        /// </summary>
        public bool? ContinueInCaseOf3DSecureError { get; set; }

        public string DefaultLanguage { get; set; }

        public bool? Donation { get; set; }

        private bool? allowInstallments;

        private bool? allowCredit;

        private bool? hidePhone;

        private bool? hideEmail;

        private bool? hideNationalID;

        private bool? allowImmediate;

        public bool? AllowInstallments { get => allowInstallments.GetValueOrDefault(true); set => allowInstallments = value; }

        public bool? AllowImmediate { get => allowImmediate.GetValueOrDefault(true); set => allowImmediate = value; }

        public bool? AllowCredit { get => allowCredit.GetValueOrDefault(true); set => allowCredit = value; }

        public bool? HidePhone { get => hidePhone.GetValueOrDefault(false); set => hidePhone = value; }

        public bool? HideEmail { get => hideEmail.GetValueOrDefault(false); set => hideEmail = value; }

        public bool? HideNationalID { get => hideNationalID.GetValueOrDefault(false); set => hideNationalID = value; }

        public bool? AllowSaveCreditCard { get; set; }

        public bool? ConsumerDataReadonly { get; set; }

        public bool? SaveCreditCardByDefault { get; set; }
    }
}
