using Merchants.Business.Entities.Billing;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Merchants.Business.Services
{
    public interface IConsumersService : IServiceBase<Consumer, Guid>
    {
        IQueryable<Consumer> GetConsumers();
    }
}
