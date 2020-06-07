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
            return (//user.HasClaim("scope", "admin_api") &&
                user?.IsInRole(Roles.BillingAdministrator) == true || user?.IsInRole(Roles.BusinessAdministrator) == true)
                   || user.IsManagementService();
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

        public static bool IsTerminal(this ClaimsPrincipal user)
        {
            return user?.FindFirst("client_id")?.Value == "terminal" && user?.FindFirst(Claims.TerminalIDClaim)?.Value != null;
        }

        public static bool IsMerchantFrontend(this ClaimsPrincipal user)
        {
            return user?.FindFirst("client_id")?.Value == "merchant_frontend" && user?.IsMerchant() == true;
        }

        public static Guid? GetMerchantID(this ClaimsPrincipal user)
        {
            var merchantID = user?.FindFirst(Claims.MerchantIDClaim)?.Value;
            if (string.IsNullOrWhiteSpace(merchantID))
            {
                return null;
            }

            if (Guid.TryParse(merchantID, out var guid))
            {
                return guid;
            }

            return null;
        }

        public static Guid? GetTerminalID(this ClaimsPrincipal user)
        {
            var terminalID = user?.FindFirst(Claims.TerminalIDClaim)?.Value;
            if (string.IsNullOrWhiteSpace(terminalID))
            {
                return null;
            }

            if (Guid.TryParse(terminalID, out var guid))
            {
                return guid;
            }

            return null;
        }

        public static string GetDoneBy(this ClaimsPrincipal user)
        {
            var name = user?.FindFirst(Claims.NameClaim)?.Value;

            var doneBy = !string.IsNullOrWhiteSpace(name) ? name : "Service";

            return doneBy;
        }

        public static Guid? GetDoneByID(this ClaimsPrincipal user)
        {
            var userId = user?.FindFirst(Claims.UserIdClaim)?.Value;

            if (userId == null)
            {
                userId = user?.FindFirst(Claims.SubjClaim)?.Value;
            }

            if (Guid.TryParse(userId, out var guid))
            {
                return guid;
            }

            return null;
        }
    }
}