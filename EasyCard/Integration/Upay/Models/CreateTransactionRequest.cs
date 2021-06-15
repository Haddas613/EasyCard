using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Upay.Models
{
    /// <summary>
    /// Create Transaction Request
    /// </summary>
    [DataContract]
    public partial class CreateTransactionRequest
    {
        public string CellPhone { get; set; }
        public string Amount { get; set; }
        public string EmailUser { get; set; }
        public string NumberPayments { get; set; }
        public string PaymentDate { get; set; }
        public string Token { get; set; }
        public string CommissionReduction { get; set; }// model.CheckUpayCommission? "0" : "1",
        public string AcceptedTransaction { get; set; } //model.CutomerConfirm ? "1" : "0",
        public string Currency { get; set; }
    }
}
