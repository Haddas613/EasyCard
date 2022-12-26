using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityServer.Helpers
{
    public class EmailTemplateCodes
    {
        public const string ConfirmationEmail = "ConfirmationEmail";
        public const string ResetPasswordEmail = "ResetPasswordEmail";
        public const string TwoFactorAuth = "2fa";
        public const string UserLinkedToMerchant = "UserLinkedToMerchant";
    }
}
