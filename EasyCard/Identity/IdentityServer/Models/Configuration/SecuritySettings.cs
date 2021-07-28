using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models.Configuration
{
    public class SecuritySettings
    {
        /// <summary>
        /// Password lifetime
        /// </summary>
        public int PasswordExpirationDays { get; set; } = 90;

        /// <summary>
        /// Remember N last passwords that user had previously used
        /// </summary>
        public int RememberLastPasswords { get; set; } = 4;
    }
}
