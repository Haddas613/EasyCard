using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Data;
using Transactions.Business.Entities;

namespace Transactions.Business.Services
{
    public class PaymentRequestsService : ServiceBase<PaymentRequest, Guid>, IPaymentRequestsService
    {
        private readonly TransactionsContext context;
        private readonly IHttpContextAccessorWrapper httpContextAccessor;

        public PaymentRequestsService(TransactionsContext context, IHttpContextAccessorWrapper httpContextAccessor)
            : base(context)
        {
            this.context = context;
            this.httpContextAccessor = httpContextAccessor;
        }

        public async override Task CreateEntity(PaymentRequest entity, IDbContextTransaction dbTransaction = null)
        {
            entity.ApplyAuditInfo(httpContextAccessor);

            await base.CreateEntity(entity, dbTransaction);
        }

        public IQueryable<PaymentRequest> GetPaymentRequests() => context.PaymentRequests;

        public async override Task UpdateEntity(PaymentRequest entity, IDbContextTransaction dbTransaction = null)
        {
            //TODO: audit
            await base.UpdateEntity(entity, dbTransaction);
        }
    }
}
