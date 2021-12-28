using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Claims;

namespace Shared.Helpers.Security
{
    public static class SecurityHelpers
    {
        /// <summary>
        /// Represents any administrative level access (interactive admin user and management service), but excludes impersomated admin
        /// </summary>
        public static bool IsAdmin(this ClaimsPrincipal user)
        {
            return ((user?.IsInRole(Roles.BillingAdministrator) == true || user?.IsInRole(Roles.BusinessAdministrator) == true) && user?.IsMerchantFrontend() == false)
                   || user.IsManagementService();
        }

        /// <summary>
        /// Interactive admin users including impersonated
        /// </summary>
        public static bool IsInteractiveAdmin(this ClaimsPrincipal user)
        {
            return user?.IsInRole(Roles.BillingAdministrator) == true || user?.IsInRole(Roles.BusinessAdministrator) == true;
        }

        /// <summary>
        /// Noninteractive asmin api access
        /// </summary>
        public static bool IsManagementService(this ClaimsPrincipal user)
        {
            return user.HasClaim("scope", "management_api");
        }

        //public static bool IsImpersonatedAdmin(this ClaimsPrincipal user)
        //{
        //    return (user?.IsInRole(Roles.Merchant) == true) &&
        //        (user?.IsInRole(Roles.BillingAdministrator) == true || user?.IsInRole(Roles.BusinessAdministrator) == true);
        //}

        public static bool IsBillingAdmin(this ClaimsPrincipal user)
        {
            return user?.IsInRole(Roles.BillingAdministrator) == true;
        }

        /// <summary>
        /// Includes inpersonated admin
        /// </summary>
        public static bool IsMerchant(this ClaimsPrincipal user)
        {
            return user?.IsInRole(Roles.Merchant) == true;
        }

        public static bool IsManager(this ClaimsPrincipal user)
        {
            return user?.IsInRole(Roles.Manager) == true;
        }

        public static bool IsUpayApi(this ClaimsPrincipal user)
        {
            return user?.IsInRole(Roles.UPayAPI) == true;
        }

        public static bool IsNayaxApi(this ClaimsPrincipal user)
        {
            return user?.IsInRole(Roles.NayaxAPI) == true;
        }

        /// <summary>
        /// Non-interactive api access
        /// </summary>
        public static bool IsTerminal(this ClaimsPrincipal user)
        {
            return user?.FindFirst("client_id")?.Value == "terminal" && user?.FindFirst(Claims.TerminalIDClaim)?.Value != null;
        }

        public static void CheckTerminalPermission(this ClaimsPrincipal user, Guid terminalID)
        {
            if (user.IsAdmin())
            {
                return;
            }

            //if (user.IsTerminal() && terminalID != user.GetTerminalID())
            //{
            //    throw new SecurityException("User has no access to requiested data");
            //}
        }

        /// <summary>
        /// Interactive merchant access, including impersonated admin
        /// </summary>
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

        public static IEnumerable<Guid> GetTerminalID(this ClaimsPrincipal user)
        {
            var terminals = user?.FindAll(Claims.TerminalIDClaim).Select(d => Guid.Parse(d.Value));

            return terminals;
        }

        public static string GetDoneBy(this ClaimsPrincipal user)
        {
            var name = user?.FindFirst(Claims.NameClaim)?.Value;

            var doneBy = !string.IsNullOrWhiteSpace(name) ? name : "Service";

            return doneBy;
        }

        public static string GetDoneByName(this ClaimsPrincipal user)
        {
            var name = user?.FindFirst(Claims.NameClaim)?.Value;
            var firstName = user?.FindFirst(Claims.FirstNameClaim)?.Value;
            var lastName = user?.FindFirst(Claims.LastNameClaim)?.Value;
            var fullName = $"{firstName} {lastName}";

            var doneBy = !string.IsNullOrWhiteSpace(fullName) ? fullName : (!string.IsNullOrWhiteSpace(name) ? name : "Service");

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