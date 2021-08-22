using Merchants.Business.Entities.Terminal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Api.Configuration;
using Shared.Business.Security;
using Shared.Helpers;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Notifications;
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
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public NotificationService(
            IOptions<ApplicationSettings> applicationSettings,
            IOptions<ApiSettings> apiSettings,
            IWebApiClient webApiClient,
            ILogger<NotificationService> logger,
            IHttpContextAccessorWrapper httpContextAccessor)
        {
            this.applicationSettings = applicationSettings.Value;
            this.webApiClient = webApiClient;
            this.logger = logger;
            this.httpContextAccessor = httpContextAccessor;
            this.apiSettings = apiSettings.Value;
            RegisterNotifiers();
        }

        public async Task NotifyTransactionStatus(PaymentTransaction transaction, Terminal terminal)
        {
            var payload = new TransactionStatusNotification
            {
                PaymentTransactionID = transaction.PaymentTransactionID,
                Status = transaction.Status,
                UserID = httpContextAccessor.GetUser()?.GetDoneByID()
            };

            foreach (var notifier in notifiers)
            {
                try
                {
                    _ = Task.Run(async () => await notifier.Notify(NotificationTypesEnum.TransactionStatus, payload, terminal)).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"{nameof(NotifyTransactionStatus)} ERROR: {ex.Message}");
                }
            }
        }

        private void RegisterNotifiers()
        {
            notifiers.Add(new ProfileNotifier(apiSettings, webApiClient));
        }
    }
}
