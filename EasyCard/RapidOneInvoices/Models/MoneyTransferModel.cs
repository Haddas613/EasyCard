using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class MoneyTransferModel
    {
        [DataMember(Name = "value")]
        public decimal Value { get; set; }

        [DataMember(Name = "dueDate")]
        public string DueDate { get; set; }

        [DataMember(Name = "bank")]
        public BankDetailsModel Bank { get; set; }

        [DataMember(Name = "bankId")]
        public int BankId { get; set; }

        [DataMember(Name = "branch")]
        public string Branch { get; set; }

        [DataMember(Name = "account")]
        public string Account { get; set; }

        [DataMember(Name = "accountNumber")]
        public string AccountNumber { get; set; }

        public MoneyTransferModel()
        {
            this.DueDate = DateTime.Now.ToString();
        }
    }
}
