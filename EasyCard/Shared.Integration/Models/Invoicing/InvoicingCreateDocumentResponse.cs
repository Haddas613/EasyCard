using Newtonsoft.Json.Linq;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Invoicing
{
    public class InvoicingCreateDocumentResponse
    {
        public InvoicingCreateDocumentResponse()
        {
            Success = true;
        }

        public string DocumentNumber { get; set; }

        public string DownloadUrl { get; set; }

        public string CopyDonwnloadUrl { get; set; }

        public bool Success { get; set; }

        /// <summary>
        /// General error mesage which can be displayed to merchant
        /// </summary>
        public string ErrorMessage { get; set; }

        public IEnumerable<Error> Errors { get; set; }

        public int? OriginalHttpResponseCode { get; set; }

        public JObject ExternalSystemData { get; set; }
    }
}
