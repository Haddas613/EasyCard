using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Helpers.Queue
{
    public interface IQueue
    {
        Task PushToQueue<T>(string queueName, T model);

        Task PushToQueue<T>(string queueName, IEnumerable<T> items);
    }
}
