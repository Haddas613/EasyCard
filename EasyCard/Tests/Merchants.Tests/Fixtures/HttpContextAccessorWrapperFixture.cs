﻿using Shared.Business.Security;
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

        public HttpContextAccessorWrapperFixture()
        {
            UserClaims = new ClaimsPrincipal();

            UserFirstNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserLastNameClaim = Guid.NewGuid().ToString().Substring(0, 8);
            UserIdClaim = Guid.NewGuid().ToString();

            UserClaims.AddIdentity(new ClaimsIdentity(new List<Claim>
            {
                new Claim(SecurityHelpers.FirstNameClaim, UserFirstNameClaim),
                new Claim(SecurityHelpers.LastNameClaim, UserLastNameClaim),
                new Claim(SecurityHelpers.UserIdClaim, UserIdClaim)
            }));
        }

        public ClaimsPrincipal GetUser()
        {
            return UserClaims;
        }
    }
}
