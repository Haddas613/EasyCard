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

        public string TraceIdentifier => throw new NotImplementedException();

        public HttpContextAccessorWrapperFixture()
        {
            UserClaims = new ClaimsPrincipal();

            UserFirstNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserLastNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserIdClaim = Guid.NewGuid();
            UserIp = Guid.NewGuid().ToString();

            TerminalIdClaimValue = Guid.NewGuid();
            MerchantIdClaimValue = Guid.NewGuid();

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

        public HttpContextAccessorWrapperFixture SetTerminalIDClaim(Guid value)
        {
            var claim = UserClaims.FindFirst(Claims.TerminalIDClaim);

            (UserClaims.Identity as ClaimsIdentity).RemoveClaim(claim);

            TerminalIdClaimValue = value;
            (UserClaims.Identity as ClaimsIdentity).AddClaim(new Claim(Claims.TerminalIDClaim, value.ToString()));

            return this;
        }

        public HttpContextAccessorWrapperFixture SetMerchantIDClaim(Guid value)
        {
            var claim = UserClaims.FindFirst(Claims.TerminalIDClaim);

            (UserClaims.Identity as ClaimsIdentity).RemoveClaim(claim);

            TerminalIdClaimValue = value;
            (UserClaims.Identity as ClaimsIdentity).AddClaim(new Claim(Claims.TerminalIDClaim, value.ToString()));

            return this;
        }

        /// <summary>
        /// Clears up current role and sets to BillingAdministrator
        /// </summary>
        /// <returns>this</returns>
        public HttpContextAccessorWrapperFixture SetRoleToBillingAdministrator()
        {
            var claim = UserClaims.FindFirst(ClaimTypes.Role);

            if (claim != null)
            {
                (UserClaims.Identity as ClaimsIdentity).RemoveClaim(claim);
            }

            (UserClaims.Identity as ClaimsIdentity).AddClaim(new Claim("scope", "admin_api"));
            (UserClaims.Identity as ClaimsIdentity).AddClaim(new Claim(ClaimTypes.Role, Roles.BillingAdministrator));

            return this;
        }

        /// <summary>
        /// Clears up current role and sets to Terminal
        /// </summary>
        /// <returns>this</returns>
        public HttpContextAccessorWrapperFixture SetRoleToTerminal(Guid terminalID)
        {
            var claim = UserClaims.FindFirst(ClaimTypes.Role);

            if (claim != null)
            {
                (UserClaims.Identity as ClaimsIdentity).RemoveClaim(claim);
            }

            (UserClaims.Identity as ClaimsIdentity).AddClaim(new Claim("client_id", "terminal"));

            return SetTerminalIDClaim(terminalID);
        }
    }
}
