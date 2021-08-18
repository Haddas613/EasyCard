using Shared.Business;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class NayaxTransactionsParameters : IEntityBase<Guid>
    {
        public NayaxTransactionsParameters()
        {
            NayaxTransactionsParametersTimestamp = DateTime.UtcNow;
            NayaxTransactionsParametersID = Guid.NewGuid().GetSequentialGuid(NayaxTransactionsParametersTimestamp.Value);
        }

        public Guid NayaxTransactionsParametersID { get; set; }

        public DateTime? NayaxTransactionsParametersTimestamp { get; set; }

        public string PinPadTransactionID { get; set; }

        /// <summary>
        /// Shva TranRecord
        /// </summary>
        public string TranRecord { get; set; }

        public Guid GetID()
        {
            return NayaxTransactionsParametersID;
        }
    }
}
