using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ProfileApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MerchantProfileApi.Hubs
{
    [Authorize(AuthenticationSchemes = "Bearer", Policy = Policy.MerchantFrontend)]
    public class TransactionsHub : Hub<ITransactionsHub>
    {
        private static readonly Dictionary<string, string> Connections = new Dictionary<string, string>();

        public override Task OnConnectedAsync()
        {
            var ctx = this.Context;
            return base.OnConnectedAsync();
        }

        public async Task MapConnection(string userID)
        {
            Connections[userID] = this.Context.ConnectionId;
        }

        public async Task TransactionStatusChanged(object payload)
        {
            await Clients.All.TransactionStatusChanged(payload);
        }
    }
}
