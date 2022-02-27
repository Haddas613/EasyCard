using System;
using System.Collections.Generic;
using System.Text;

namespace Ecwid.Api.Models
{
    public class EcwidUpdateOrderStatusRequest
    {
        /// <summary>
        /// ECNG transaction ID
        /// </summary>
        public Guid PaymentTransactionID { get; set; }

        public string ReferenceTransactionID { get; set; }

        public string StoreID { get; set; }

        public string Token { get; set; }

        public EcwidOrderStatusEnum Status { get; set; }

        public string CorrelationId { get; set; }
    }
}
