using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class CompanyDto
    {
        public string Name { get; set; }
        public string DbName { get; set; }
        public string Locale { get; set; }
        public string AccountingType { get; set; }
        public bool InvoiceReceiptModeOnly { get; set; }
        public bool ReceiptModeOnly { get; set; }
        public bool ChargeFieldActive { get; set; }
        public string DepositAccountsToBranches { get; set; }
        public bool? IsDefaultForCurrentBranch { get; set; }
        public bool? SetItemsFromPricelistAsCharged { get; set; }
        public string ExportCode { get; set; }
    }
}
