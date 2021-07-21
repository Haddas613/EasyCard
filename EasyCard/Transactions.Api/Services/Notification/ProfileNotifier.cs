using Shared.Api.Configuration;
using Shared.Api.Models;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared;

namespace Transactions.Api.Services.Notification
{
    public class ProfileNotifier : INotifier
    {
        private readonly ApiSettings apiSettings;
        private readonly IWebApiClient webApiClient;

        public ProfileNotifier(ApiSettings apiSettings, IWebApiClient webApiClient)
        {
            this.apiSettings = apiSettings;
            this.webApiClient = webApiClient;
        }

        public Task Notify(NotificationTypesEnum type, object payload, object settings) => (type, payload, settings) switch
        {
            (NotificationTypesEnum.TransactionStatus, _, _) => NotifyTransactionStatus(payload),
            _ => Task.FromResult(false)
        };

        public Task NotifyTransactionStatus(object payload)
        {
            var uri = new UriBuilder(apiSettings.MerchantsManagementApiAddress);
            uri.Path = "external/notifications/transaction-status";

            return webApiClient.Post<OperationResponse>(uri.Uri.GetLeftPart(UriPartial.Authority), uri.Path, payload, null, null, null);
        }
    }
}
