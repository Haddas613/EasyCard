using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Models.Billing
{
    public class ConsumerResponse
    {
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

        public string ConsumerAddress { get; set; }

        public string ConsumerNationalID { get; set; }

        public DateTime? Created { get; set; }

        public string OperationDoneBy { get; set; }

        public string CorrelationId { get; set; }
    }
}
