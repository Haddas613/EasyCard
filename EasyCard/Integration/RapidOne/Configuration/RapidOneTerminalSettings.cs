using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOne
{
    public class RapidOneTerminalSettings
    {
        public string BaseUrl { get; set; }
        public string Token { get; set; }
        public string Company { get; set; }
        public int Department { get; set; }
        public int Branch { get; set; }
        public string BankAccountNumber { get; set; }
        public string LedgerAccount { get; set; }
        public bool Charge { get; set; }
    }
}
