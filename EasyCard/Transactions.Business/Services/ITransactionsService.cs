﻿using Microsoft.EntityFrameworkCore.Storage;
using Shared.Business;
using Shared.Integration.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Transactions.Business.Entities;
using Transactions.Shared.Enums;

namespace Transactions.Business.Services
{
    public interface ITransactionsService : IServiceBase<PaymentTransaction, Guid>
    {
        IQueryable<PaymentTransaction> GetTransactions();

        IQueryable<TransactionHistory> GetTransactionHistory(Guid transactionID);

        Task UpdateEntityWithStatus(PaymentTransaction entity, TransactionStatusEnum? transactionStatus = null, TransactionFinalizationStatusEnum? finalizationStatus = null, RejectionReasonEnum? rejectionReason = null, string rejectionMessage = null, IDbContextTransaction dbTransaction = null);

        Task<IEnumerable<TransmissionInfo>> StartTransmission(Guid terminalID, IEnumerable<Guid> transactionIDs, IDbContextTransaction dbTransaction = null);
    }
}
