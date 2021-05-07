using Shared.Helpers.Queue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api
{
    public class QueueResolver : IQueueResolver
    {
        public const string InvoiceQueue = "invoice";
        public const string BillingDealsQueue = "billingdeals";

        private Dictionary<string, IQueue> queues = new Dictionary<string, IQueue>();

        public QueueResolver(IQueue invoiceQueue)
        {
            queues.Add(InvoiceQueue, invoiceQueue);
        }

        public IQueue GetQueue(string queueName)
        {
            return queues[queueName];
        }
    }
}
