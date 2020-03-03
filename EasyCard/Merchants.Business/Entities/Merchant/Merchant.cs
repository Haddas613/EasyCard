using Shared.Business;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Merchant
{
    public class Merchant : IEntityBase
    {
        public Merchant()
        {
            //Terminals = new HashSet<Merchants.Business.Entities.Terminal.Terminal>();
        }

        public long MerchantID { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public string BusinessName { get; set; }

        public string MarketingName { get; set; }

        public string BusinessID { get; set; }

        public string ContactPerson { get; set; }

        // lets try to live without collections
        //public ICollection<Merchants.Business.Entities.Terminal.Terminal> Terminals { get; set; }

        public string Users { get; set; }

        public DateTime? Created { get; set; }

        public long GetID()
        {
            return MerchantID;
        }
    }
}
