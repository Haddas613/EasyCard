using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Billing
{
    public class Consumer : IAuditEntity, IEntityBase<Guid>
    {
        public Consumer()
        {
            Created = DateTime.UtcNow;
            ConsumerID = Guid.NewGuid().GetSequentialGuid(Created.Value);
        }

        public Guid ConsumerID { get; set; }

        public Guid MerchantID { get; set; }

        public Merchant.Merchant Merchant { get; set; }

        public bool Active { get; set; }

        public byte[] UpdateTimestamp { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        public string ConsumerName { get; set; }

        /// <summary>
        /// End-customer Phone
        /// </summary>
        public string ConsumerPhone { get; set; }

        public string ConsumerAddress { get; set; }

        public DateTime? Created { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        public Guid GetID()
        {
            return ConsumerID;
        }
    }
}
