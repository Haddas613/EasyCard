using IdentityModel;
using System;
using System.Linq;
using System.Security.Claims;

namespace Shared.Helpers.Security
{
    public static class SecurityHelpers
    {
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return user.HasClaim("scope", "admin_api")
                   && (user?.IsInRole(Roles.BillingAdministrator) == true || user?.IsInRole(Roles.BusinessAdministrator) == true);
        }

        public static bool IsManagementService(this ClaimsPrincipal user)
        {
            return user.HasClaim("scope", "management_api");
        }

        public static bool IsImpersonatedAdmin(this ClaimsPrincipal user)
        {
            return user.HasClaim("scope", "merchant_api") &&
                (user?.IsInRole(Roles.BillingAdministrator) == true || user?.IsInRole(Roles.BusinessAdministrator) == true);
        }

        public static bool IsBillingAdmin(this ClaimsPrincipal user)
        {
            return user.HasClaim("scope", "admin_api") &&
                   user?.IsInRole(Roles.BillingAdministrator) == true;
        }

        public static bool IsMerchant(this ClaimsPrincipal user)
        {
            return (user?.IsInRole(Roles.Merchant) == true) || user?.IsImpersonatedAdmin() == true;
        }

        public static long? GetMerchantID(this ClaimsPrincipal user)
        {
            var merchantID = user?.FindFirst(Claims.MerchantIDClaim)?.Value;
            if (string.IsNullOrWhiteSpace(merchantID))
            {
                return null;
            }

            return Convert.ToInt64(merchantID);
        }

        public static string GetDoneBy(this ClaimsPrincipal user)
        {
            var firstName = user?.FindFirst(Claims.FirstNameClaim)?.Value;
            var lastName = user?.FindFirst(Claims.LastNameClaim)?.Value;
            var fullName = (firstName + " " + lastName).Trim();

            var doneBy = !string.IsNullOrWhiteSpace(fullName) ? fullName : "Service";

            return doneBy;
        }

        public static string GetDoneByID(this ClaimsPrincipal user)
        {
            var userId = user?.FindFirst(Claims.UserIdClaim)?.Value;

            if (userId == null)
            {
                userId = user?.FindFirst(Claims.SubjClaim)?.Value;
            }

            return userId;
        }
    }
}