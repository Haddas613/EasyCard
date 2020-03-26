using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Text;

namespace ClearingHouse
{
    public class ClearingHouseGlobalSettings : IdentityServerClientSettings
    {
        /// <summary>
        /// Payment gateway ID
        /// </summary>
        public int? PaymentGatewayID { get; set; }

        public string ApiBaseAddress { get; set; }
    }
}
