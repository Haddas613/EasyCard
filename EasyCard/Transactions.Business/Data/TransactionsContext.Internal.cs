﻿using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Business.Security;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using Transactions.Business.Entities;
using Shared.Helpers.Security;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Security;
using System.Data;
using Dapper;
using Transactions.Shared.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Transactions.Shared.Models;
using System.Linq;
using Shared.Integration.Models;
using Shared.Business.Extensions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging.Debug;
using Shared.Integration.Models.PaymentDetails;
using Transactions.Shared;

namespace Transactions.Business.Data
{
    public partial class TransactionsContext
    {
        public async Task<IEnumerable<TransactionSummaryDb>> GetGroupedTransactionSummaries(Guid? terminalID, IDbContextTransaction dbTransaction = null)
        {
            var builder = new SqlBuilder();

            var query = @"select TOP (@maxRecords) PaymentTransactionID, TerminalID, MerchantID, TransactionAmount, TransactionType, Currency, TransactionTimestamp, Status, SpecialTransactionType, JDealType, RejectionReason, CardPresence, CardOwnerName, TransactionDate, NumberOfRecords, ShvaDealID 
from(
    select PaymentTransactionID, TerminalID, MerchantID, TransactionAmount, TransactionType, Currency, TransactionTimestamp, Status, SpecialTransactionType, JDealType, RejectionReason, CardPresence, CardOwnerName, TransactionDate, r = row_number() over(partition by TransactionDate order by PaymentTransactionID desc), NumberOfRecords = count(*) over(partition by TransactionDate), ShvaDealID
    from dbo.PaymentTransaction WITH(NOLOCK) /**where**/
    ) a
where r <= @pageSize
 order by PaymentTransactionID desc";

            var selector = builder.AddTemplate(query, new { maxRecords = 100, pageSize = 10 }); // TODO: use config

            var jDealType = JDealTypeEnum.J4;

            builder.Where($"{nameof(PaymentTransaction.JDealType)} = @{nameof(jDealType)}", new { jDealType });

            if (terminalID.HasValue)
            {
                user.CheckTerminalPermission(terminalID.Value);
                builder.Where($"{nameof(PaymentTransaction.TerminalID)} = @{nameof(terminalID)}", new { terminalID });
            }

            if (user.IsAdmin())
            {
            }
            else if (user.IsTerminal() && !terminalID.HasValue)
            {
                builder.Where($"{nameof(PaymentTransaction.TerminalID)} = @{nameof(terminalID)}", new { terminalID = user.GetTerminalID() });
            }
            else if (user.IsMerchant())
            {
                var merchantID = user.GetMerchantID();
                builder.Where($"{nameof(PaymentTransaction.MerchantID)} = @{nameof(merchantID)}", new { merchantID });
            }
            else
            {
                throw new SecurityException("User has no access to requested data");
            }

            var connection = this.Database.GetDbConnection();
            bool existingConnection = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    existingConnection = false;
                }

                var report = await connection.QueryAsync<TransactionSummaryDb>(selector.RawSql, selector.Parameters, transaction: dbTransaction?.GetDbTransaction());

                return report;
            }
            finally
            {
                if (!existingConnection)
                {
                    connection.Close();
                }
            }
        }

        public async Task<IEnumerable<TransmissionInfo>> StartTransmission(Guid terminalID, IEnumerable<Guid> transactionIDs, IDbContextTransaction dbTransaction = null)
        {
            user.CheckTerminalPermission(terminalID);

            string query = @"
DECLARE @OutputTransactionIDs table(
    [PaymentTransactionID] [uniqueidentifier] NULL,
    [ShvaDealID] [varchar](50) NULL,
    [ShvaTerminalID] [varchar](20) NULL,
    [ShvaTranRecord] [varchar](700) NULL
);

UPDATE t SET t.[Status]=@NewStatus, t.[UpdatedDate]=@UpdatedDate, t.ShvaTranRecord = ISNULL(t.ShvaTranRecord, n.ShvaTranRecord)
OUTPUT inserted.PaymentTransactionID, inserted.ShvaDealID, inserted.ShvaTerminalID, inserted.ShvaTranRecord INTO @OutputTransactionIDs
FROM [dbo].[PaymentTransaction] as t LEFT OUTER JOIN [dbo].[NayaxTransactionsParameters] as n on n.PinPadTransactionID = t.PinPadTransactionID
WHERE t.[PaymentTransactionID] in @TransactionIDs AND t.[TerminalID] = @TerminalID AND t.[Status]=@OldStatus and ISNULL(t.ShvaTranRecord, n.ShvaTranRecord) is not null";

            // security check
            // TODO: replace to query builder

            if (user.IsAdmin())
            {
            }
            else if (user.IsTerminal())
            {
                var userTerminalID = user.GetTerminalID()?.FirstOrDefault();
                if (terminalID != userTerminalID)
                {
                    throw new SecurityException("User has no access to requested data");
                }
            }
            else if (user.IsMerchant())
            {
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    if (!terminals.Contains(terminalID))
                    {
                        throw new SecurityException("User has no access to requested data");
                    }
                }

                query += " AND [MerchantID] = @MerchantID";
            }
            else
            {
                throw new SecurityException("User has no access to requested data");
            }

            query += @" SELECT PaymentTransactionID, ShvaDealID, ShvaTerminalID, ShvaTranRecord as TranRecord from @OutputTransactionIDs as a";

            var connection = this.Database.GetDbConnection();
            bool existingConnection = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    existingConnection = false;
                }

                var report = await connection.QueryAsync<TransmissionInfo>(query, new { NewStatus = TransactionStatusEnum.TransmissionInProgress, OldStatus = TransactionStatusEnum.AwaitingForTransmission, TerminalID = terminalID, MerchantID = user.GetMerchantID(), TransactionIDs = transactionIDs, UpdatedDate = DateTime.UtcNow }, transaction: dbTransaction?.GetDbTransaction());

                return report;
            }
            finally
            {
                if (!existingConnection)
                {
                    connection.Close();
                }
            }
        }

        public async Task<IEnumerable<Guid>> StartSendingInvoices(Guid terminalID, IEnumerable<Guid> invoicesIDs, IDbContextTransaction dbTransaction)
        {
            user.CheckTerminalPermission(terminalID);

            string query = @"
DECLARE @OutputInvoiceIDs table(
    [InvoiceID] [uniqueidentifier] NULL
);

UPDATE [dbo].[Invoice] SET [Status]=@NewStatus, [UpdatedDate]=@UpdatedDate 
OUTPUT inserted.InvoiceID INTO @OutputInvoiceIDs
WHERE [InvoiceID] in @InvoicesIDs AND [TerminalID] = @TerminalID AND [Status]<>@OldStatus";

            // security check
            // TODO: replace to query builder

            if (user.IsAdmin())
            {
            }
            else if (user.IsTerminal())
            {
                var userTerminalID = user.GetTerminalID()?.FirstOrDefault();
                if (terminalID != userTerminalID)
                {
                    throw new SecurityException("User has no access to requested data");
                }
            }
            else if (user.IsMerchant())
            {
                var terminals = user.GetTerminalID();
                if (terminals?.Count() > 0)
                {
                    if (!terminals.Contains(terminalID))
                    {
                        throw new SecurityException("User has no access to requested data");
                    }
                }

                query += " AND [MerchantID] = @MerchantID";
            }
            else
            {
                throw new SecurityException("User has no access to requested data");
            }

            query += @"
SELECT InvoiceID from @OutputInvoiceIDs as a";

            var connection = this.Database.GetDbConnection();
            bool existingConnection = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    existingConnection = false;
                }

                var report = await connection.QueryAsync<Guid>(query, new { NewStatus = InvoiceStatusEnum.Sending, OldStatus = InvoiceStatusEnum.Sending, TerminalID = terminalID, MerchantID = user.GetMerchantID(), InvoicesIDs = invoicesIDs, UpdatedDate = DateTime.UtcNow }, transaction: dbTransaction?.GetDbTransaction());

                return report;
            }
            finally
            {
                if (!existingConnection)
                {
                    connection.Close();
                }
            }
        }

        public async Task<long> GenerateMasavFile(Guid? merchantID, Guid? terminalID, string institueName, int? sendingInstitute, string instituteNumber, DateTime? masavFileDate)
        {
            var connection = this.Database.GetDbConnection();
            bool connectionOpened = connection.State == ConnectionState.Open;
            try
            {
                if (!connectionOpened)
                {
                    await connection.OpenAsync();
                }

                var query = "[dbo].[PR_GenerateMasavFile]";

                var args = new DynamicParameters(new { FileDate = masavFileDate, MerchantID = merchantID, TerminalID = terminalID, InstitueName = institueName, InstituteNumber = instituteNumber, SendingInstitute = sendingInstitute, PaymentTypeEnum = PaymentTypeEnum.Bank, Currency = CurrencyEnum.ILS, TransactionStatusOld = TransactionStatusEnum.Initial, TransactionStatusNew = TransactionStatusEnum.Completed });

                args.Add("@Error", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);
                args.Add("@MasavFileID", dbType: DbType.Int64, direction: ParameterDirection.Output, size: -1);

                var result = await connection.ExecuteAsync(query, args, commandType: CommandType.StoredProcedure);

                var error = args.Get<string>("@Error");

                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new ApplicationException(error);
                }

                var res = args.Get<long?>("@MasavFileID");

                if (!res.HasValue)
                {
                    throw new BusinessException(Messages.TerminalHasNoTransactionsToGenerateMasavFile);
                }

                return res.Value;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!connectionOpened)
                {
                    connection.Close();
                }
            }
        }

        public async Task<IEnumerable<FutureBilling>> GetFutureBillings(Guid? merchantID, Guid? terminalID, Guid? consumerID, Guid? billingDealID, DateTime? startDate, DateTime? endDate)
        {
            var connection = this.Database.GetDbConnection();
            bool connectionOpened = connection.State == ConnectionState.Open;
            try
            {
                if (!connectionOpened)
                {
                    await connection.OpenAsync();
                }

                var query = "[dbo].[PR_FutureBillings]";

                var args = new DynamicParameters(new { merchantID, terminalID, consumerID, billingDealID, startDate, endDate });

                var result = await connection.QueryAsync<FutureBilling>(query, args, commandType: CommandType.StoredProcedure);

                return result;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (!connectionOpened)
                {
                    connection.Close();
                }
            }
        }

        public async Task<bool> CheckDuplicateTransaction(Guid? terminalID, Guid? paymentIntentID, Guid? paymentRequestID, DateTime? threshold, decimal amount, string cardNumber, IDbContextTransaction dbContextTransaction, JDealTypeEnum jDealType)
        {
            if (!paymentIntentID.HasValue && !paymentRequestID.HasValue && (!threshold.HasValue || string.IsNullOrWhiteSpace(cardNumber)))
            {
                return false;
            }

            var builder = new SqlBuilder();

            var query = @"select TOP (1) PaymentTransactionID from dbo.PaymentTransaction /**where**/";

            var selector = builder.AddTemplate(query);

            builder.Where($"{nameof(PaymentTransaction.TerminalID)} = @{nameof(terminalID)} and [{nameof(PaymentTransaction.Status)}] >= 0 and {nameof(PaymentTransaction.JDealType)} = @{nameof(jDealType)}", new { terminalID, jDealType });

            bool isOrFilter = (paymentIntentID.HasValue || paymentRequestID.HasValue) && threshold.HasValue && !string.IsNullOrWhiteSpace(cardNumber);

            if (paymentIntentID.HasValue)
            {
                var filter = $"{nameof(PaymentTransaction.PaymentIntentID)} = @{nameof(paymentIntentID)}";
                if (isOrFilter)
                {
                    builder.OrWhere(filter, new { paymentIntentID });
                }
                else
                {
                    builder.Where(filter, new { paymentIntentID });
                }
            }

            if (paymentRequestID.HasValue && !paymentIntentID.HasValue)
            {
                var filter = $"{nameof(PaymentTransaction.PaymentRequestID)} = @{nameof(paymentRequestID)}";
                if (isOrFilter)
                {
                    builder.OrWhere(filter, new { paymentRequestID });
                }
                else
                {
                    builder.Where(filter, new { paymentRequestID });
                }
            }

            if (threshold.HasValue && !string.IsNullOrWhiteSpace(cardNumber))
            {
                var filter = $"{nameof(PaymentTransaction.TransactionTimestamp)} >= @{nameof(threshold)} and {nameof(PaymentTransaction.TransactionAmount)} = @{nameof(amount)} and {nameof(PaymentTransaction.CreditCardDetails.CardNumber)} = @{nameof(cardNumber)}";

                if (isOrFilter)
                {
                    builder.OrWhere(filter, new { threshold, amount, cardNumber });
                }
                else
                {
                    builder.Where(filter, new { threshold, amount, cardNumber });
                }
            }

            var connection = this.Database.GetDbConnection();
            bool existingConnection = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    existingConnection = false;
                }

                var report = await connection.ExecuteScalarAsync<Guid?>(selector.RawSql, selector.Parameters, transaction: dbContextTransaction?.GetDbTransaction());

                return report.HasValue;
            }
            finally
            {
                if (!existingConnection)
                {
                    connection.Close();
                }
            }
        }

        public async Task<bool> CheckDuplicateBillingDeal(BillingDealCompare billingDealCompare, DateTime? threshold, PaymentTypeEnum paymentType, IDbContextTransaction dbContextTransaction)
        {
            if (!threshold.HasValue)
            {
                return false;
            }

            var builder = new SqlBuilder();

            var query = @"select TOP (1) BillingDealID from dbo.BillingDeal /**where**/";

            var selector = builder.AddTemplate(query);

            _ = builder.Where(
                $"{nameof(BillingDeal.TerminalID)} = @{nameof(billingDealCompare.TerminalID)} and " +
                $"{nameof(BillingDeal.MerchantID)}  = @{nameof(billingDealCompare.MerchantID)}  and " +
                $"{nameof(BillingDeal.PaymentType)} = @{nameof(paymentType)}",

                new { billingDealCompare.TerminalID, billingDealCompare.MerchantID, paymentType });

            if (threshold.HasValue)
            {
                var filter = $"{nameof(BillingDeal.BillingDealTimestamp)} >= @{nameof(threshold)}";

                builder.Where(filter, new { threshold });
            }
            if (billingDealCompare.OperationDoneByID.HasValue)
            {
                var filter = $"{nameof(BillingDeal.OperationDoneByID)} = @{nameof(billingDealCompare.OperationDoneByID)}";

                builder.Where(filter, new { billingDealCompare.OperationDoneByID });
            }

            var connection = this.Database.GetDbConnection();
            bool existingConnection = true;
            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                    existingConnection = false;
                }

                var report = await connection.ExecuteScalarAsync<Guid?>(selector.RawSql, selector.Parameters, transaction: dbContextTransaction?.GetDbTransaction());

                return report.HasValue;
            }
            finally
            {
                if (!existingConnection)
                {
                    connection.Close();
                }
            }
        }
    }
}
