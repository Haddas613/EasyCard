using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Shared.Helpers;
using Shared.Integration.Models;
using Shared.Integration.Models.Invoicing;
using Shared.Integration.Models.PaymentDetails;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Transactions;
using Transactions.Shared.Models;
using IntegrationModels = Shared.Integration.Models;
using TransactionsApi = Transactions.Api;

namespace Transactions.Api.Models.Invoicing
{
    public class UpdateInvoiceRequest
    {
        /// <summary>
        /// Primary reference
        /// </summary>
        public Guid InvoiceID { get; set; }

        /// <summary>
        /// Deal information (optional)
        /// </summary>
        public IntegrationModels.DealDetails DealDetails { get; set; }
    }
}
