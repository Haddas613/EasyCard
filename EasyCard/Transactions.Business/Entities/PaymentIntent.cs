using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class PaymentIntent : TableEntity
    {
        private readonly string partitionKey = "1";

        public PaymentIntent()
        {
        }

        public PaymentIntent(Guid terminalID, Guid paymentIntentID)
        {
            this.PartitionKey = partitionKey;
            this.TerminalID = terminalID;
            this.PaymentIntentID = paymentIntentID;
        }

        public string PaymentRequest { get; set; }

        public Guid? TerminalID { get; set; }

        public Guid? PaymentIntentID
        {
            get => Guid.Parse(this.RowKey);

            set
            {
                this.RowKey = value.ToString();
            }
        }
    }
}
