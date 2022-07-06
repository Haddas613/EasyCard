using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    public class InvoicingCancelDocumentRequest
    {
        public string InvoiceID { get; set; }

        public object InvoiceingSettings { get; set; }

        /// <summary>
        /// Request ID
        /// </summary>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Invoice details
        /// </summary>
        public InvoiceDetails InvoiceDetails { get; set; }

        public Newtonsoft.Json.Linq.JObject Extension { get; set; }

        public bool Donation { get; set; }

        public string InvoiceNumber { get; set; }
    }
}
