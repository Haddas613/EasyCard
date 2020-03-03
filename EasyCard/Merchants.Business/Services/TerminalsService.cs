using Merchants.Business.Data;
using Merchants.Business.Entities.Terminal;
using Microsoft.EntityFrameworkCore;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class TerminalsService : ServiceBase<Terminal>, ITerminalsService
    {
        private readonly MerchantsContext context;

        public TerminalsService(MerchantsContext context)
            : base(context)
        {
            this.context = context;
        }

        public IQueryable<Terminal> GetTerminals()
        {
            return context.Terminals;
        }

        public async Task LinkUserToTerminal(string userID, long terminalID)
        {
            var terminal = await context.Terminals.FirstAsync(t => t.TerminalID == terminalID);

            //TODO: provide missing data
            context.UserTerminalMappings.Add(new Entities.User.UserTerminalMapping
            {
                OperationDate = DateTime.UtcNow,
                OperationDoneBy = null,
                OperationDoneByID = null,
                TerminalID = terminalID,
                UserID = userID,
            });

            await context.SaveChangesAsync();
        }

        public async Task UnLinkUserFromTerminal(string userID, long terminalID)
        {
            var terminal = await context.Terminals.FirstAsync(t => t.TerminalID == terminalID);

            var entity = await context.UserTerminalMappings.FirstAsync(m => m.TerminalID == terminalID && m.UserID == userID);

            context.UserTerminalMappings.Remove(entity);

            await context.SaveChangesAsync();
        }
    }
}
