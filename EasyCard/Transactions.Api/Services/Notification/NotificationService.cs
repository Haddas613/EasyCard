using Merchants.Business.Entities.Terminal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared;

namespace Transactions.Api.Services.Notification
{
    public class NotificationService : INotificationService
    {
        private readonly HashSet<INotifier> notifiers = new HashSet<INotifier>();
        private readonly ApplicationSettings applicationSettings;
        private readonly ApiSettings apiSettings;
        private readonly IWebApiClient webApiClient;
        private readonly ILogger logger;

        public NotificationService(
            IOptions<ApplicationSettings> applicationSettings,
            IOptions<ApiSettings> apiSettings,
            IWebApiClient webApiClient,
            ILogger<NotificationService> logger)
        {
            this.applicationSettings = applicationSettings.Value;
            this.webApiClient = webApiClient;
            this.logger = logger;
            RegisterNotifiers();
        }

        public async Task NotifyTransactionStatus(PaymentTransaction transaction, Terminal terminal)
        {
            foreach (var notifier in notifiers)
            {
                _ = notifier.Notify(NotificationTypesEnum.TransactionStatus, transaction, terminal);
            }
        }

        private void RegisterNotifiers()
        {
            notifiers.Add(new ProfileNotifier(apiSettings, webApiClient));
        }
    }
}
