using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class ApplicationSettings
    {
        public int ConfirmationEmailExpirationInHours { get; set; } = 72;

        public int ResetPasswordEmailExpirationInHours { get; set; } = 4;

        public bool ForgotPasswordCheckBankAccountNumber { get; set; }

        public object CompanyName { get; set; }

        public string EmailEventHubConnectionString { get; set; }

        public string EmailEventHubName { get; set; }

        public string InternalCertificate { get; set; }
    }
}
