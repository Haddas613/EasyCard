using Shared.Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Models.Invoicing
{
    public class DownloadInvoiceResponse : OperationResponse
    {
        public DownloadInvoiceResponse(IEnumerable<string> downloadLinks)
        {
            this.DownloadLinks = downloadLinks;
        }

        public IEnumerable<string> DownloadLinks { get; set; }
    }
}
