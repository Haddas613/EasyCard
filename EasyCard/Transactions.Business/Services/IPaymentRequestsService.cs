﻿using Microsoft.EntityFrameworkCore.Storage;
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
    public interface IPaymentRequestsService : IServiceBase<PaymentRequest, Guid>
    {
        IQueryable<PaymentRequest> GetPaymentRequests();

        IQueryable<PaymentRequestHistory> GetPaymentRequestHistory(Guid paymentRequestID);

        Task UpdateEntityWithStatus(PaymentRequest entity, PaymentRequestStatusEnum? status = null, string message = null, Guid? paymentTransactionID = null, IDbContextTransaction dbTransaction = null);
    }
}
