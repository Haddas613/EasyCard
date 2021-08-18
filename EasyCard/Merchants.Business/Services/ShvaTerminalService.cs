using Merchants.Business.Data;
using Merchants.Business.Entities.Integration;
using Microsoft.EntityFrameworkCore;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class ShvaTerminalService : ServiceBase<ShvaTerminal, string>, IShvaTerminalsService
    {
        private readonly MerchantsContext context;

        public ShvaTerminalService(MerchantsContext context)
            : base(context)
        {
            this.context = context;
        }

        public Task<ShvaTerminal> GetShvaTerminal(string userName)
            => context.ShvaTerminals.FirstOrDefaultAsync(t => t.UserName == userName);

        public IQueryable<ShvaTerminal> GetShvaTerminals()
            => context.ShvaTerminals.AsNoTracking();
    }
}
