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

        public string EmailTableName { get; set; }

        public string EmailQueueName { get; set; }

        public string InternalCertificate { get; set; }

        public string DefaultStorageConnectionString { get; set; }

        public string RequestsLogStorageTable { get; set; }

        public string SmsTableName { get; set; }

        public string SmsFrom { get; set; }

        public bool TwoFactorAuthenticationDoNotSendSms { get; set; }

        public string SendRegistrationRequestEmailsTo { get; set; }
    }
}
