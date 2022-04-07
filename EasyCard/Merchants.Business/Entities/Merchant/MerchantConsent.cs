using Merchants.Shared.Enums;
using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class MerchantConsent : IEntityBase<Guid>
    {
        public MerchantConsent()
        {
            Created = DateTime.UtcNow;
            MerchantConsentID = Guid.NewGuid().GetSequentialGuid(Created);
        }

        public ConsentTypeEnum ConsentType { get; set; }

        public Guid MerchantConsentID { get; set; }

        public DateTime Created { get; set; }

        public string ConsentText { get; set; }

        public string ButtonText { get; set; }

        public Guid MerchantID { get; set; }

        public Guid? TerminalID { get; set; }

        public string UserName { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public Guid GetID() => MerchantConsentID;
    }
}
