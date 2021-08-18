using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Transactions.Hubs
{
    [Authorize(AuthenticationSchemes = "SignalR")]
    public class TransactionsHub : Hub<ITransactionsHub>
    {
        private static readonly Dictionary<Guid, string> Connections = new Dictionary<Guid, string>();

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task MapConnection(Guid userID)
        {
            Connections[userID] = Context.ConnectionId;
        }

        //public async Task TransactionStatusChanged(Object payload)
        //{
        //    if (!Connections.ContainsKey(payload.UserID))
        //    {
        //        return;
        //    }

        //    await Clients.Client(Connections[payload.UserID]).TransactionStatusChanged(Connections[payload.UserID]);
        //}
    }
}
