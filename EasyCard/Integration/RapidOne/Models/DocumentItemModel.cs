using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class DocumentItemModel
    {
        public string url { get; set; }

        public int docEntry { get; set; }
        /// <summary>
        ///  Invoices = 13,
        ///CreditNotes = 14,
        ///DeliveryNotes = 15,
        ///Returns = 16,
        ///IncomingPayments = 24
        /// </summary>
        public int docType { get; set; }
    }
}
