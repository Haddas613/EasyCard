using System;

namespace IdentityServer.Models
{
    public class MerchantLogin
    {
        public Guid? MerchantID { get; set; }

        public string MerchantName { get; set; }
    }
}
