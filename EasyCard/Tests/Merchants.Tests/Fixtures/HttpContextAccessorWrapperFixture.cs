using Microsoft.AspNetCore.Http;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Merchants.Tests.Fixtures
{
    // TODO: use constructor to add roles, merchantID etc.
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
        public string UserIdClaim { get; set; }

        /// <summary>
        /// Get or set current user IP
        /// </summary>
        public string UserIp { get; set; }

        public HttpContextAccessorWrapperFixture()
        {
            UserClaims = new ClaimsPrincipal();

            UserFirstNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserLastNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserIdClaim = Guid.NewGuid().ToString();
            UserIp = Guid.NewGuid().ToString();

            UserClaims.AddIdentity(new ClaimsIdentity(new List<Claim>
            {
                new Claim(Claims.FirstNameClaim, UserFirstNameClaim),
                new Claim(Claims.LastNameClaim, UserLastNameClaim),
                new Claim(Claims.UserIdClaim, UserIdClaim)
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
