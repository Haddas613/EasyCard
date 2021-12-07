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
        public const string UpdateTerminalSHVAParametersQueue = "updateterminalshvaparameters";
        public const string TransmissionQueue = "transmission";

        private readonly Dictionary<string, IQueue> queues = new Dictionary<string, IQueue>();

        public QueueResolver()
        {
        }

        public QueueResolver AddQueue(string key, IQueue queue)
        {
            queues.Add(key, queue);

            return this;
        }

        public IQueue GetQueue(string queueName)
        {
            return queues[queueName];
        }
    }
}
