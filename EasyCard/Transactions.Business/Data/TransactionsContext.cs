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

namespace Transactions.Business.Data
{
    public class TransactionsContext : DbContext
    {
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<CreditCardTokenDetails> CreditCardTokenDetails { get; set; }

        public DbSet<TransactionHistory> TransactionHistories { get; set; }

        public DbSet<BillingDeal> BillingDeals { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<PaymentRequest> PaymentRequests { get; set; }

        public DbSet<PaymentRequestHistory> PaymentRequestHistories { get; set; }

        private readonly ClaimsPrincipal user;

        private static readonly ValueConverter CardExpirationConverter = new ValueConverter<CardExpiration, string>(
            v => v.ToString(),
            v => CreditCardHelpers.ParseCardExpiration(v));

        private static readonly ValueConverter ItemsConverter = new ValueConverter<IEnumerable<Item>, string>(
           v => JsonConvert.SerializeObject(v),
           v => JsonConvert.DeserializeObject<IEnumerable<Item>>(v));

        private static readonly ValueComparer ItemsComparer = new ValueComparer<IEnumerable<Item>>(
          (c1, c2) => c1 != null ? c1.SequenceEqual(c2) : false,
          c => c != null ? c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())) : 0,
          c => c != null ? c.ToList() : c);

        private static readonly ValueConverter CustomerAddressConverter = new ValueConverter<Address, string>(
           v => JsonConvert.SerializeObject(v),
           v => JsonConvert.DeserializeObject<Address>(v));

        private static readonly ValueConverter StringsArrayConverter = new ValueConverter<string[], string>(
          v => v == null ? null : string.Join(",", v),
          v => v == null ? null : v.Split(",", StringSplitOptions.RemoveEmptyEntries));

        private static readonly ValueComparer StringArrayComparer = new ValueComparer<string[]>(
           (c1, c2) => c1.SequenceEqual(c2),
           c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
           c => (string[])c.Clone());

        public TransactionsContext(DbContextOptions<TransactionsContext> options, IHttpContextAccessorWrapper httpContextAccessor)
            : base(options)
        {
            this.user = httpContextAccessor.GetUser();
        }

        // NOTE: use this for debugging purposes to analyse sql query performance
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder
        //        .UseLoggerFactory(DbCommandConsoleLoggerFactory);

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
    [ShvaDealID] [varchar](50) NULL
);

UPDATE [dbo].[PaymentTransaction] SET [Status]=@NewStatus, [UpdatedDate]=@UpdatedDate 
OUTPUT inserted.PaymentTransactionID, inserted.ShvaDealID INTO @OutputTransactionIDs
WHERE [PaymentTransactionID] in @TransactionIDs AND [TerminalID] = @TerminalID AND [Status]=@OldStatus";

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
SELECT PaymentTransactionID, ShvaDealID from @OutputTransactionIDs as a";

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Address>();

            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new CreditCardTokenDetailsConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new BillingDealConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentRequestConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentRequestHistoryConfiguration());

            // NOTE: security filters moved to Get() methods

            base.OnModelCreating(modelBuilder);
        }

        internal class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
        {
            public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
            {
                builder.ToTable("PaymentTransaction");

                builder.HasKey(b => b.PaymentTransactionID);
                builder.Property(b => b.PaymentTransactionID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(r => r.TransactionDate).IsRequired(true).HasColumnType("date");

                builder.Property(p => p.TerminalID).IsRequired(true);
                builder.Property(p => p.MerchantID).IsRequired(true);

                builder.Property(p => p.CreditCardToken).HasColumnName("CreditCardToken");

                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter).HasColumnName("CardExpiration");
                    s.Property(p => p.CardNumber).HasColumnName("CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                    s.Property(p => p.CardBin).HasColumnName("CardBin").IsRequired(false).HasMaxLength(10).IsUnicode(false);
                    s.Property(p => p.CardVendor).HasColumnName("CardVendor").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                });

                builder.OwnsOne(b => b.ClearingHouseTransactionDetails, s =>
                {
                    s.Property(p => p.ClearingHouseTransactionID).HasColumnName("ClearingHouseTransactionID");
                    s.Property(p => p.MerchantReference).HasColumnName("ClearingHouseMerchantReference").IsRequired(false).HasMaxLength(50);
                    s.Ignore(p => p.ConcurrencyToken);
                });

                builder.OwnsOne(b => b.ShvaTransactionDetails, s =>
                {
                    s.Property(p => p.ShvaDealID).HasColumnName("ShvaDealID").IsRequired(false).HasMaxLength(30).IsUnicode(false);
                    s.Property(p => p.ShvaShovarNumber).HasColumnName("ShvaShovarNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ManuallyTransmitted).HasColumnName("ManuallyTransmitted");
                    s.Property(p => p.ShvaTerminalID).HasColumnName("ShvaTerminalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ShvaTransmissionNumber).HasColumnName("ShvaTransmissionNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.TransmissionDate).HasColumnName("ShvaTransmissionDate").IsRequired(false);
                    s.Property(p => p.Solek).HasColumnName("Solek").IsRequired(false);
                });

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                    s.Property(p => p.CustomerAddress).HasColumnName("CustomerAddress").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(CustomerAddressConverter);
                });

                builder.Property(p => p.ConsumerIP).HasColumnName("ConsumerIP").IsRequired(false).HasMaxLength(32).IsUnicode(false);
                builder.Property(p => p.MerchantIP).HasColumnName("MerchantIP").IsRequired(false).HasMaxLength(32).IsUnicode(false);

                builder.Property(b => b.TransactionAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InstallmentPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InitialPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.VATRate).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.VATTotal).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.NetTotal).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.TerminalTemplateID).IsRequired(false);
            }
        }

        internal class CreditCardTokenDetailsConfiguration : IEntityTypeConfiguration<CreditCardTokenDetails>
        {
            public void Configure(EntityTypeBuilder<CreditCardTokenDetails> builder)
            {
                builder.ToTable("CreditCardTokenDetails");

                builder.HasKey(b => b.CreditCardTokenID);
                builder.Property(b => b.CreditCardTokenID).ValueGeneratedNever();

                builder.Property(p => p.TerminalID).IsRequired(true);
                builder.Property(p => p.MerchantID).IsRequired(true);

                builder.Property(b => b.CardNumber).IsRequired(true).HasMaxLength(20).IsUnicode(false);
                builder.Property(b => b.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter);
                builder.Property(b => b.CardVendor).HasMaxLength(20).IsRequired(false).IsUnicode(false);
                builder.Property(b => b.CardOwnerNationalID).HasMaxLength(20).IsRequired(false).IsUnicode(false);
                builder.Property(b => b.CardOwnerName).HasMaxLength(50).IsRequired(false).IsUnicode(true);
                builder.Property(b => b.Active);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);
                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Ignore(b => b.CardReaderInput);

                builder.OwnsOne(b => b.ShvaInitialTransactionDetails, s =>
                {
                    s.Property(p => p.ShvaDealID).HasColumnName("ShvaDealID").IsRequired(false).HasMaxLength(30).IsUnicode(false);
                    s.Property(p => p.AuthNum).HasColumnName("AuthNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.AuthSolekNum).HasColumnName("AuthSolekNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ShvaTransactionDate).HasColumnName("ShvaTransactionDate").IsRequired(false);
                });

                builder.Property(p => p.ConsumerEmail).IsRequired(false).HasMaxLength(50).IsUnicode(false);
            }
        }

        internal class TransactionHistoryConfiguration : IEntityTypeConfiguration<TransactionHistory>
        {
            public void Configure(EntityTypeBuilder<TransactionHistory> builder)
            {
                builder.ToTable("TransactionHistory");

                builder.HasKey(b => b.TransactionHistoryID);
                builder.Property(b => b.TransactionHistoryID).ValueGeneratedNever();

                builder.Property(b => b.PaymentTransactionID).IsRequired();

                builder.Property(b => b.OperationDate).IsRequired();

                builder.Property(b => b.OperationCode).IsRequired().HasMaxLength(30).IsUnicode(false);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.OperationDescription).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);

                builder.Property(b => b.OperationMessage).IsRequired(false).HasMaxLength(250).IsUnicode(true);

                builder.Property(b => b.CorrelationId).IsRequired().HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);
            }
        }

        internal class BillingDealConfiguration : IEntityTypeConfiguration<BillingDeal>
        {
            public void Configure(EntityTypeBuilder<BillingDeal> builder)
            {
                builder.ToTable("BillingDeal");

                builder.HasKey(b => b.BillingDealID);
                builder.Property(b => b.BillingDealID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(p => p.TerminalID).IsRequired(true);
                builder.Property(p => p.MerchantID).IsRequired(true);

                builder.Property(p => p.CreditCardToken).HasColumnName("CreditCardToken");

                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter).HasColumnName("CardExpiration");
                    s.Property(p => p.CardNumber).HasColumnName("CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                    s.Property(p => p.CardBin).HasColumnName("CardBin").IsRequired(false).HasMaxLength(10).IsUnicode(false);
                    s.Property(p => p.CardVendor).HasColumnName("CardVendor").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                });

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                });

                builder.OwnsOne(b => b.InvoiceDetails, s =>
                {
                    s.Property(p => p.InvoiceType).HasColumnName("InvoiceType");

                    s.Property(p => p.InvoiceSubject).HasColumnName("InvoiceSubject").IsRequired(false).IsUnicode(true);
                    s.Property(p => p.SendCCTo).HasColumnName("SendCCTo").IsRequired(false).IsUnicode(true).HasConversion(StringsArrayConverter)
                        .Metadata.SetValueComparer(StringArrayComparer);
                });

                builder.Property(b => b.TransactionAmount).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.VATRate).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.VATTotal).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.NetTotal).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(p => p.BillingSchedule).HasColumnName("BillingSchedule").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasJsonConversion();

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);
            }
        }

        internal class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
        {
            public void Configure(EntityTypeBuilder<Invoice> builder)
            {
                builder.ToTable("Invoice");

                builder.HasKey(b => b.InvoiceID);
                builder.Property(b => b.InvoiceID).ValueGeneratedNever();

                builder.Property(p => p.InvoiceNumber).HasColumnName("InvoiceNumber").IsRequired(false).HasMaxLength(20).IsUnicode(true);

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(p => p.TerminalID).IsRequired(true);
                builder.Property(p => p.MerchantID).IsRequired(true);

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                });

                builder.OwnsOne(b => b.InvoiceDetails, s =>
                {
                    s.Property(p => p.InvoiceType).HasColumnName("InvoiceType");
                    s.Property(p => p.InvoiceSubject).HasColumnName("InvoiceSubject").IsRequired(false).IsUnicode(true);
                    s.Property(p => p.SendCCTo).HasColumnName("SendCCTo").IsRequired(false).IsUnicode(true).HasConversion(StringsArrayConverter)
                        .Metadata.SetValueComparer(StringArrayComparer);
                });

                builder.Property(b => b.InvoiceAmount).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.VATRate).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.VATTotal).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.NetTotal).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                builder.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);

                builder.Property(b => b.CopyDonwnloadUrl).IsRequired(false).IsUnicode(false);
                builder.Property(b => b.DownloadUrl).IsRequired(false).IsUnicode(false);

                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter).HasColumnName("CardExpiration");
                    s.Property(p => p.CardNumber).HasColumnName("CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                    s.Ignore(p => p.CardBin);
                    s.Property(p => p.CardVendor).HasColumnName("CardVendor").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                });

                builder.Property(b => b.InstallmentPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InitialPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").IsRequired();
            }
        }

        internal class PaymentRequestConfiguration : IEntityTypeConfiguration<PaymentRequest>
        {
            public void Configure(EntityTypeBuilder<PaymentRequest> builder)
            {
                builder.ToTable("PaymentRequest");

                builder.HasKey(b => b.PaymentRequestID);
                builder.Property(b => b.PaymentRequestID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(p => p.TerminalID).IsRequired(true);
                builder.Property(p => p.MerchantID).IsRequired(true);

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                });

                builder.OwnsOne(b => b.InvoiceDetails, s =>
                {
                    s.Property(p => p.InvoiceType).HasColumnName("InvoiceType");
                    s.Property(p => p.InvoiceSubject).HasColumnName("InvoiceSubject").IsRequired(false).IsUnicode(true);
                    s.Property(p => p.SendCCTo).HasColumnName("SendCCTo").IsRequired(false).IsUnicode(true).HasConversion(StringsArrayConverter)
                        .Metadata.SetValueComparer(StringArrayComparer);
                });

                builder.Property(b => b.PaymentRequestAmount).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.VATRate).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.VATTotal).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.NetTotal).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.FromAddress).IsRequired(false).HasMaxLength(100).IsUnicode(true);
                builder.Property(b => b.RequestSubject).IsRequired(false).HasMaxLength(250).IsUnicode(true);

                builder.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                builder.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);

                builder.Property(b => b.InstallmentPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InitialPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").IsRequired();
            }
        }

        internal class PaymentRequestHistoryConfiguration : IEntityTypeConfiguration<PaymentRequestHistory>
        {
            public void Configure(EntityTypeBuilder<PaymentRequestHistory> builder)
            {
                builder.ToTable("PaymentRequestHistory");

                builder.HasKey(b => b.PaymentRequestHistoryID);
                builder.Property(b => b.PaymentRequestHistoryID).ValueGeneratedNever();

                builder.Property(b => b.PaymentRequestID).IsRequired();

                builder.Property(b => b.OperationDate).IsRequired();

                builder.Property(b => b.OperationCode).IsRequired().HasMaxLength(30).IsUnicode(false);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.OperationDescription).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);

                builder.Property(b => b.OperationMessage).IsRequired(false).HasMaxLength(250).IsUnicode(true);

                builder.Property(b => b.CorrelationId).IsRequired().HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);
            }
        }
    }
}
