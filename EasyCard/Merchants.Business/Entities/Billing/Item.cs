using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Billing
{
    public class Item : IAuditEntity, IEntityBase<Guid>, IMerchantEntity
    {
        public Item()
        {
            Created = DateTime.UtcNow;
            ItemID = Guid.NewGuid().GetSequentialGuid(Created.Value);
        }

        public Guid ItemID { get; set; }

        public Guid MerchantID { get; set; }

        public Merchant.Merchant Merchant { get;  set; }

        public bool Active { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        public string ItemName { get; set; }

        public decimal Price { get; set; }

        public CurrencyEnum Currency { get; set; }

        public DateTime? Created { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public string ExternalReference { get; set; }

        public string BillingDesktopRefNumber { get; set; }

        public Guid GetID()
        {
            return ItemID;
        }
    }
}
