using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Terminal
{
    public class TerminalCheckoutSettingsUpdate
    {
        // TODO: move to other place (sepends on implementation)
        public string CustomCssReference { get; set; }

        // TODO: validation
        public IEnumerable<string> RedirectUrls { get; set; }

        public bool LegacyRedirectResponse { get; set; }

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
    }
}
