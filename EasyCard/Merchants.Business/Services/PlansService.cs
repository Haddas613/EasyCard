using Merchants.Business.Data;
using Merchants.Business.Entities.Merchant;
using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Merchants.Business.Services
{
    public class PlansService : ServiceBase<Plan, long>, IPlansService
    {
        private readonly MerchantsContext context;

        public PlansService(MerchantsContext context)
            : base(context)
        {
            this.context = context;
        }

        public IQueryable<Plan> GetQuery() => context.Plans;
    }
}
