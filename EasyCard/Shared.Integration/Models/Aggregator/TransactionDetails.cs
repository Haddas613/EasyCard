using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Integration.Models.Aggregator
{
   public class TransactionDetails
    {
        public string Amount { get; set; }

        public string CardCompany { get; set; }

        public string OwnerPhoneNumber { get; set; }

        public string OwnerIdentityNumber { get; set; }

        public string PaymentsNumber { get; set; }

        public string ExternalID { get; set; }
    }
}
