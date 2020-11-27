using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Helpers.Queue
{
    public interface IQueueResolver
    {
        IQueue GetQueue(string queueName);
    }
}
