using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class RapidOneCreateDocumentResponse
    {
        public RapidOneCreateDocumentResponse()
        {

        }

        public RapidOneCreateDocumentResponse(IEnumerable<DocumentItemModel> documents)
        {
            Documents = documents;
        }

        public IEnumerable<DocumentItemModel> Documents { get; set; }
    }

    public class RapidOneCreateDocumentErrorResponse
    {
        public string Error { get; set; }
    }

    public class DocumentItemModel
    {
        public string Url { get; set; }

        public int DocEntry { get; set; }

        /// <summary>
        ///  Invoices = 13,
        ///  CreditNotes = 14,
        ///  DeliveryNotes = 15,
        ///  Returns = 16,
        ///  IncomingPayments = 24
        /// </summary>
        public int DocType { get; set; }

        public string IssuerDB { get; set; }

        public string DocNumber { get; set; }

        public string Lang { get; set; }
    }
}
