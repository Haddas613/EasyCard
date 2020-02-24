using MerchantsApi.Business.Data;
using MerchantsApi.Business.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MerchantsApi.Business.Services
{
    public class TerminalsService : ITerminalsService
    {

        private readonly MerchantsContext context;
        public TerminalsService(MerchantsContext context)
        {
            this.context = context;
        }

        public IQueryable<Terminal> GetTerminals()
        {
            return context.Terminals;
        }
    }
}
