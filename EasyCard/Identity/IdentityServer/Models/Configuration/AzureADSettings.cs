using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Models
{
    public class AzureADSettings
    {
        public string AzureAdAuthority { get; set; }

        public string AzureAdClientId { get; set; }

        public string AzureAdBusinessAdministratorGrpId { get; set; }

        public string AzureAdBillingAdministratorGrpId { get; set; }
    }
}
