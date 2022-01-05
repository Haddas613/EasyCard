using System;
using System.Collections.Generic;
using System.Text;

namespace EasyInvoice.Models
{
    public class ECInvoiceGetDocumentTaxReportRequest
    {

        public EasyInvoiceTerminalSettings Terminal { get; set; }
        public string StartDate { get; set; }//, from.ToString("yyyy-MM-dd"));
        public string EndDate { get; set; }//to.ToString("yyyy-MM-dd"));
    }
}
