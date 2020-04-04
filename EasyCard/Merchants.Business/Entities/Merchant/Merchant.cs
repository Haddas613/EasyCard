using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class Merchant : IEntityBase<Guid>
    {
        public Merchant()
        {
            //Terminals = new HashSet<Merchants.Business.Entities.Terminal.Terminal>();
            Created = DateTime.UtcNow;
            MerchantID = Guid.NewGuid().GetSequentialGuid(Created.Value);
        }

        public Guid MerchantID { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public string BusinessName { get; set; }

        public string MarketingName { get; set; }

        public string BusinessID { get; set; }

        public string ContactPerson { get; set; }

        // lets try to live without collections
        //public ICollection<Merchants.Business.Entities.Terminal.Terminal> Terminals { get; set; }

        public string Users { get; set; }

        public DateTime? Created { get; set; }

        public Guid GetID()
        {
            return MerchantID;
        }
    }
}
