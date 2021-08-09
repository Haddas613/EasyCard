using Microsoft.Azure.Cosmos.Table;
using System;
using System.Collections.Generic;
using System.Text;

namespace Transactions.Business.Entities
{
    public class PaymentIntent : TableEntity
    {
        public PaymentIntent()
        {
        }

        public PaymentIntent(Guid terminalID, Guid paymentIntentID)
        {
            this.TerminalID = terminalID;
            this.PaymentIntentID = paymentIntentID;
        }

        public string PaymentRequest { get; set; }

        public Guid? TerminalID
        {
            get => Guid.Parse(this.PartitionKey);

            set
            {
                this.PartitionKey = value.ToString();
            }
        }

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
