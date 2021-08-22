using Microsoft.AspNetCore.Http;
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

namespace Transactions.Business.Data
{
    public partial class TransactionsContext
    {
        public async Task<IEnumerable<TransactionSummaryDb>> GetGroupedTransactionSummaries(Guid? terminalID, IDbContextTransaction dbTransaction = null)
        {
            var builder = new SqlBuilder();

            var query = @"select TOP (@maxRecords) PaymentTransactionID, TerminalID, MerchantID, TransactionAmount, TransactionType, Currency, TransactionTimestamp, Status, SpecialTransactionType, JDealType, RejectionReason, CardPresence, CardOwnerName, TransactionDate, NumberOfRecords
from(
    select PaymentTransactionID, TerminalID, MerchantID, TransactionAmount, TransactionType, Currency, TransactionTimestamp, Status, SpecialTransactionType, JDealType, RejectionReason, CardPresence, CardOwnerName, TransactionDate, r = row_number() over(partition by TransactionDate order by PaymentTransactionID desc), NumberOfRecords = count(*) over(partition by TransactionDate)
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
    [ShvaTranRecord] [varchar](500) NULL
);

UPDATE t SET t.[Status]=@NewStatus, t.[UpdatedDate]=@UpdatedDate, t.ShvaTranRecord = ISNULL(t.ShvaTranRecord, n.ShvaTranRecord)
OUTPUT inserted.PaymentTransactionID, inserted.ShvaDealID, inserted.ShvaTerminalID, inserted.ShvaTranRecord INTO @OutputTransactionIDs
FROM [dbo].[PaymentTransaction] as t LEFT OUTER JOIN [dbo].[NayaxTransactionsParameters] as n on n.PinPadTransactionID = t.PinPadTransactionID
WHERE t.[PaymentTransactionID] in @TransactionIDs AND t.[TerminalID] = @TerminalID AND t.[Status]=@OldStatus";

            // security check
            // TODO: replace to query builder

            if (user.IsAdmin())
            {
            }
            else if (user.IsTerminal())
            {
                if (terminalID != user.GetTerminalID())
                {
                    throw new SecurityException("User has no access to requested data");
                }
            }
            else if (user.IsMerchant())
            {
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
                if (terminalID != user.GetTerminalID())
                {
                    throw new SecurityException("User has no access to requested data");
                }
            }
            else if (user.IsMerchant())
            {
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

        public async Task<long> GenerateMasavFile(Guid? terminalID, int? bank, int? bankBranch, string bankAccount, DateTime? masavFileDate)
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

                var args = new DynamicParameters(new { FileDate = masavFileDate, TerminalID = terminalID, InstitueName = bank.ToString(), InstituteNumber = bankAccount, SendingInstitute = bankBranch, PaymentTypeEnum = PaymentTypeEnum.Bank, Currency = CurrencyEnum.ILS });

                args.Add("@Error", dbType: DbType.String, direction: ParameterDirection.Output, size: -1);
                args.Add("@MasavFileID", dbType: DbType.Int64, direction: ParameterDirection.Output, size: -1);

                var result = await connection.ExecuteAsync(query, args, commandType: CommandType.StoredProcedure);

                var error = args.Get<string>("@Error");

                if (!string.IsNullOrWhiteSpace(error))
                {
                    throw new ApplicationException(error);
                }

                var res = args.Get<long>("@MasavFileID");

                return res;
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
    }
}
