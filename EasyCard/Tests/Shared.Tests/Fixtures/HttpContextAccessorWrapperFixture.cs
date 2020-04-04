using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Shared.Tests.Fixtures
{
    public class HttpContextAccessorWrapperFixture : IHttpContextAccessorWrapper
    {
        /// <summary>
        /// Get or set user claims
        /// </summary>
        public ClaimsPrincipal UserClaims { get; set; }

        /// <summary>
        /// Get or set current user First Name claim Value
        /// </summary>
        public string UserFirstNameClaim { get; set; }

        /// <summary>
        /// Get or set current user Last Name claim Value
        /// </summary>
        public string UserLastNameClaim { get; set; }

        /// <summary>
        /// Get or set current user Id claim Value
        /// </summary>
        public Guid UserIdClaim { get; set; }

        public Guid TerminalIdClaimValue { get; private set; }

        public Guid MerchantIdClaimValue { get; private set; }

        /// <summary>
        /// Get or set current user IP
        /// </summary>
        public string UserIp { get; set; }

        public HttpContextAccessorWrapperFixture(Guid terminalClaimIdValue, Guid merchantClaimIdValue)
        {
            UserClaims = new ClaimsPrincipal();

            UserFirstNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserLastNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserIdClaim = Guid.NewGuid();
            UserIp = Guid.NewGuid().ToString();

            TerminalIdClaimValue = terminalClaimIdValue;
            MerchantIdClaimValue = merchantClaimIdValue;

            UserClaims.AddIdentity(new ClaimsIdentity(new List<Claim>
            {
                new Claim(Claims.FirstNameClaim, UserFirstNameClaim),
                new Claim(Claims.LastNameClaim, UserLastNameClaim),
                new Claim(Claims.UserIdClaim, UserIdClaim.ToString()),
                new Claim(Claims.TerminalIDClaim, TerminalIdClaimValue.ToString()),
                new Claim(Claims.MerchantIDClaim, MerchantIdClaimValue.ToString()),
            }));
        }

        public ClaimsPrincipal GetUser()
        {
            return UserClaims;
        }

        public string GetIP()
        {
            return UserIp;
        }

        public string GetCorrelationId()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
