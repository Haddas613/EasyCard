using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    /// <summary>
    /// Entity to store in SQL database to control tokens operations
    /// </summary>
    public class CreditCardTokenDetails : CreditCardDetailsBase, IEntityBase<Guid>, IAuditEntity
    {
        public CreditCardTokenDetails()
        {
            Created = DateTime.UtcNow;
            CreditCardTokenID = Guid.NewGuid().GetSequentialGuid(Created.Value);
            Active = true;
        }

        public Guid CreditCardTokenID { get; set; }

        public Guid? TerminalID { get; set; }

        public Guid? MerchantID { get; set; }

        public DateTime? Created { get; set; }

        public bool Active { get; set; }

        public Guid GetID() => CreditCardTokenID;

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }
    }
}
