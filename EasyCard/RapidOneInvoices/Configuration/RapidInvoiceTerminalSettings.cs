using System;
using System.Collections.Generic;
using System.Text;

namespace RapidOneInvoices
{
    public class RapidInvoiceTerminalSettings
    {
     public string BaseUrl { get; set; }
     public string Token { get; set; }
     public string Company { get; set; }
      public string Department { get; set; }
      public string Branch { get; set; }
      public string BankAccountNumber { get; set; }
    }
}
