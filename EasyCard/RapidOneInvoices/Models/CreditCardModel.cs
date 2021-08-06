using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace RapidOneInvoices.Models
{
    [DataContract]
    public class CreditCardModel
    {
        [DataMember(Name = "value")]
        public decimal Value { get; set; }

        [DataMember(Name = "type")]
        public int Type { get; set; }
        /// <summary>
        /// 1 אשראי רגיל
        /// 2 תשלומים
        /// </summary>
        [DataMember(Name = "dealType")]
        public int DealType { get; set; }

        [DataMember(Name = "dealTypeObj")]
        public object DealTypeObj { get; set; }
        /// <summary>
        /// full card number 
        /// </summary>
        [DataMember(Name = "number")]
        public string Number { get; set; }
        [DataMember(Name = "expiration")]
        public ExpirationModel Expiration { get; set; }

        [DataMember(Name = "payments")]
        public int Payments { get; set; }

        [DataMember(Name = "firstPayment")]
        public decimal FirstPayment { get; set; }

        [DataMember(Name = "remaining")]
        public string Remaining { get; set; }

        [DataMember(Name = "voucherNum")]
        public string VoucherNum { get; set; }

        [DataMember(Name = "disabled")]
        public bool Disabled { get; set; }

        public CreditCardModel()
        {
            //this.type = 4;

            this.DealType = 1;
            this.DealTypeObj = null;
            this.Payments = default;
            this.FirstPayment = default;
            this.Remaining = null;
            this.Disabled = false;
        }
    }
}
