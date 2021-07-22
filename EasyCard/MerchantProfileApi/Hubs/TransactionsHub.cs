using MerchantProfileApi.Models.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using ProfileApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Hubs
{
    [Authorize(AuthenticationSchemes = "SignalR")]
    public class TransactionsHub : Hub<ITransactionsHub>
    {
        private static readonly Dictionary<Guid, string> Connections = new Dictionary<Guid, string>();

        public override Task OnConnectedAsync()
        {
            var ctx = this.Context;
            return base.OnConnectedAsync();
        }

        public async Task MapConnection(Guid userID)
        {
            Connections[userID] = Context.ConnectionId;
        }

        public async Task TransactionStatusChanged(TransactionsStatusRequest payload)
        {
            if (!Connections.ContainsKey(payload.UserID))
            {
                return;
            }

            await Clients.Client(Connections[payload.UserID]).TransactionStatusChanged(payload);
        }
    }
}
