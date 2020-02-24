using Merchants.Business.Data;
using Merchants.Business.Entities.Terminal;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merchants.Business.Services
{
    public class TerminalsService : ServiceBase<Terminal>, ITerminalsService
    {

        private readonly MerchantsContext context;
        public TerminalsService(MerchantsContext context) : base(context)
        {
            this.context = context;
        }

        public IQueryable<Terminal> GetTerminals()
        {
            return context.Terminals;
        }
    }
}
