using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public interface IPaymentIntentService
    {
        Task<PaymentIntentInternal> GetPaymentIntent(Guid terminalID, Guid paymentIntentID);

        Task SavePaymentIntent(PaymentIntentInternal entity);

        Task DeletePaymentIntent(Guid terminalID, Guid guid);
    }
}
