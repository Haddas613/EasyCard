using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Api.Services.Notification
{
    public interface INotifier
    {
        Task Notify(NotificationTypesEnum type, object payload, object settings);
    }
}
