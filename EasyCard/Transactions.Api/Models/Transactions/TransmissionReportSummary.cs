using Shared.Helpers;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Transactions
{
    public class TransmissionReportSummary
    {
        public Guid PaymentTransactionID { get; set; }

        public DateTime Date { get; set; }

        public CurrencyEnum Currency { get; set; }

        public decimal TotalAmount { get; set; }

        public string ShvaDealID { get; set; }

        public DateTime? TransmissionDate { get; set; }

        public SolekEnum? Solek { get; set; }

        public string ConsumerName { get; set; }

        public string ShvaTransmissionNumber { get; set; }
    }
}
