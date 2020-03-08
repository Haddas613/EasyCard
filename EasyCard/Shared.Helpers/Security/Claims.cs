using IdentityModel;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Helpers.Security
{
    public class Claims
    {
        public const string MerchantIDClaim = "extension_MerchantID";
        public const string TerminalIDClaim = "extension_TerminalID";

        public const string FirstNameClaim = "extension_FirstName";
        public const string LastNameClaim = "extension_LastName";

        public const string UserIdClaim = ClaimTypes.NameIdentifier;
        public const string SubjClaim = JwtClaimTypes.Subject;
    }
}
