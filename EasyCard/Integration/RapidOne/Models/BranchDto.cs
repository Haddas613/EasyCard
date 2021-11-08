using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne.Models
{
    public class BranchDto
    {
        public int BranchID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string EMail { get; set; }
        public string Fax { get; set; }
        public string Note { get; set; }
        public string DefaultCity { get; set; }
        public string DefaultCountry { get; set; }
        public IEnumerable<string> Issuers { get; set; }
        public string DefaultIssuer { get; set; }
        public int? DepositSeries { get; set; }
        public string DefaultBankAccount { get; set; }
        public string CashAccount { get; set; }
        public string DefaultTaxCode { get; set; }
    }
}
