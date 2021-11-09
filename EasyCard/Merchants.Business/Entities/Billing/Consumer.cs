using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Merchants.Business.Entities.Billing
{
    public class Consumer : IAuditEntity, IEntityBase<Guid>, IConcurrencyCheck, ITerminalEntity, IMerchantEntity
    {
        public Consumer()
        {
            Created = DateTime.UtcNow;
            ConsumerID = Guid.NewGuid().GetSequentialGuid(Created.Value);
        }

        public Guid ConsumerID { get; set; }

        public Guid MerchantID { get; set; }

        public Guid TerminalID { get; set; }

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

        public string ConsumeSecondPhone { get; set; }

        public string ConsumerNationalID { get; set; }

        public string ConsumerNote { get; set; }

        public Address ConsumerAddress { get; set; }

        public DateTime? Created { get; set; }

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string ExternalReference { get; set; }

        public string BillingDesktopRefNumber { get; set; }

        public string SourceIP { get; set; }

        public string Origin { get; set; }

        public Guid GetID()
        {
            return ConsumerID;
        }
    }
}
