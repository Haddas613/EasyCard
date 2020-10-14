using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntegrationModels = Shared.Integration.Models;

namespace Transactions.Api.Models.PaymentRequests
{
    public class PaymentRequestCreate
    {
        /// <summary>
        /// Terminal
        /// </summary>
        public Guid TerminalID { get; set; }

        /// <summary>
        /// Deal information (optional)
        /// </summary>
        public IntegrationModels.DealDetails DealDetails { get; set; }

    }
}
