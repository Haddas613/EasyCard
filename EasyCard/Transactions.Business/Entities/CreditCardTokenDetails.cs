﻿using Shared.Business;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using Transactions.Shared.Enums;

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

        public override CardExpiration CardExpiration
        {
            get { return CreditCardHelpers.ParseCardExpiration(ExpirationDate); } set { ExpirationDate = value?.ToDate(); }
        }

        public DateTime? ExpirationDate { get; set; }

        public bool Active { get; set; }

        public Guid GetID() => CreditCardTokenID;

        public string OperationDoneBy { get; set; }

        public Guid? OperationDoneByID { get; set; }

        public string CorrelationId { get; set; }

        public string SourceIP { get; set; }

        // TODO: this should be removed
        public ShvaInitialTransactionDetails ShvaInitialTransactionDetails { get; set; }

        /// <summary>
        /// Consumer ID
        /// </summary>
        public Guid? ConsumerID { get; set; }

        /// <summary>
        /// End-customer Email
        /// </summary>
        public string ConsumerEmail { get; set; }

        /// <summary>
        /// Reference to initial transaction
        /// </summary>
        public Guid? InitialTransactionID { get; set; }

        public DocumentOriginEnum DocumentOrigin { get; set; }

        /// <summary>
        /// For tokens report, must be set to previous token ID when updating/renewing other token
        /// </summary>
        public Guid? ReplacementOfTokenID { get; set; }

        public CardExpiration CardExpirationBeforeExtended
        {
            get { return CreditCardHelpers.ParseCardExpiration(CardExpirationBeforeExtendedDate); }
            set { CardExpirationBeforeExtendedDate = value?.ToDate(); }
        }

        public DateTime? CardExpirationBeforeExtendedDate { get; set; }

        public DateTime? Extended { get; set; }
    }
}
