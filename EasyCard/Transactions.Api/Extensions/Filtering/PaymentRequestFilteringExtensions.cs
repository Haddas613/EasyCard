using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Transactions.Api.Models.Invoicing;
using Transactions.Api.Models.PaymentRequests;
using Transactions.Business.Entities;

namespace Transactions.Api.Extensions.Filtering
{
    public static class PaymentRequestFilteringExtensions
    {
        public static IQueryable<PaymentRequest> Filter(this IQueryable<PaymentRequest> src, PaymentRequestsFilter filter)
        {
            return src;
        }
    }
}
