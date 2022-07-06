using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public interface IFutureBillingsService
    {
        Task<IEnumerable<FutureBilling>> GetFutureBillings(Guid? terminalID, Guid? consumerID, Guid? billingDealID, DateTime? startDate, DateTime? endDate);
    }
}
