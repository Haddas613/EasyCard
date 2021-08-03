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

namespace Transactions.Business.Data
{
    public class TransactionsContext : DbContext
    {
        public static readonly LoggerFactory DbCommandConsoleLoggerFactory
            = new LoggerFactory(new[]
            {
                new DebugLoggerProvider()
            });

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<CreditCardTokenDetails> CreditCardTokenDetails { get; set; }

        public DbSet<TransactionHistory> TransactionHistories { get; set; }

        public DbSet<BillingDealHistory> BillingDealHistories { get; set; }

        public DbSet<BillingDeal> BillingDeals { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<PaymentRequest> PaymentRequests { get; set; }

        public DbSet<PaymentRequestHistory> PaymentRequestHistories { get; set; }

        public DbSet<FutureBilling> FutureBillings { get; set; }

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

        private static readonly ValueConverter PaymentDetailsConverter = new ValueConverter<IEnumerable<PaymentDetails>, string>(
          v => JsonConvert.SerializeObject(v),
          v => JsonConvert.DeserializeObject<IEnumerable<PaymentDetails>>(v));

        private static readonly ValueComparer PaymentDetailsComparer = new ValueComparer<IEnumerable<PaymentDetails>>(
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
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLoggerFactory(DbCommandConsoleLoggerFactory);

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

UPDATE [dbo].[PaymentTransaction] SET [Status]=@NewStatus, [UpdatedDate]=@UpdatedDate 
OUTPUT inserted.PaymentTransactionID, inserted.ShvaDealID, inserted.ShvaTerminalID, inserted.ShvaTranRecord INTO @OutputTransactionIDs
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Ignore<Address>();

            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new CreditCardTokenDetailsConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new BillingDealConfiguration());
            modelBuilder.ApplyConfiguration(new BillingDealHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentRequestConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentRequestHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new FutureBillingConfiguration());

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
                    s.Property(p => p.CardBrand).HasColumnName("CardBrand").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                    s.Ignore(b => b.Solek);
                });

                builder.OwnsOne(b => b.ClearingHouseTransactionDetails, s =>
                {
                    s.Property(p => p.ClearingHouseTransactionID).HasColumnName("ClearingHouseTransactionID");
                    s.Property(p => p.MerchantReference).HasColumnName("ClearingHouseMerchantReference").IsRequired(false).HasMaxLength(50);
                    s.Ignore(p => p.ConcurrencyToken);
                });

                builder.OwnsOne(b => b.UpayTransactionDetails, s =>
                {
                    s.Property(p => p.CashieriD).IsRequired(false).HasMaxLength(64).HasColumnName("UpayTransactionID");
                    s.Property(p => p.CreditCardCompanyCode).IsRequired(false).HasMaxLength(64).HasColumnName("UpayCreditCardCompanyCode");
                    s.Property(p => p.MerchantNumber).IsRequired(false).HasMaxLength(64).HasColumnName("UpayMerchantNumber");
                    s.Ignore(p => p.ErrorMessage)/*.IsRequired(false).HasMaxLength(512).IsUnicode(true)*/;
                    s.Ignore(p => p.ErrorDescription)/*.IsRequired(false).HasMaxLength(512).IsUnicode(true)*/;
                    s.Property(p => p.WebUrl).IsRequired(false).HasMaxLength(512).IsUnicode(true).HasColumnName("UpayWebUrl");
                    s.Ignore(b => b.SessionID);
                    s.Ignore(b => b.TotalAmount)/*.HasColumnType("decimal(19,4)").IsRequired()*/;
                });

                builder.OwnsOne(b => b.ShvaTransactionDetails, s =>
                {
                    s.Property(p => p.ShvaDealID).HasColumnName("ShvaDealID").IsRequired(false).HasMaxLength(30).IsUnicode(false);
                    s.Property(p => p.ShvaShovarNumber).HasColumnName("ShvaShovarNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ManuallyTransmitted).HasColumnName("ManuallyTransmitted");
                    s.Property(p => p.ShvaTerminalID).HasColumnName("ShvaTerminalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ShvaTransmissionNumber).HasColumnName("ShvaTransmissionNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ShvaAuthNum).HasColumnName("ShvaAuthNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.TransmissionDate).HasColumnName("ShvaTransmissionDate").IsRequired(false);
                    s.Property(p => p.Solek).HasColumnName("Solek").IsRequired(false);
                    s.Property(p => p.TranRecord).HasColumnName("ShvaTranRecord").HasMaxLength(500).IsUnicode(false).IsRequired(false);
                    s.Property(p => p.EmvSoftVersion).HasColumnName("EmvSoftVersion").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CompRetailerNum).HasColumnName("CompRetailerNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.TelToGetAuthNum).HasColumnName("TelToGetAuthNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                });

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ExternalConsumerCode).HasColumnName("ExternalConsumerCode").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                    s.Property(p => p.ConsumerAddress).HasColumnName("CustomerAddress").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(CustomerAddressConverter);
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
                builder.Property(b => b.TotalDiscount).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.TerminalTemplateID).IsRequired(false);

                builder.Property(p => p.PinPadDeviceID).HasColumnName("PinPadDeviceID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
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
                    s.Property(p => p.CardBrand).HasColumnName("CardBrand").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.Solek).HasColumnName("Solek").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                });

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ExternalConsumerCode).HasColumnName("ExternalConsumerCode").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                    s.Property(p => p.ConsumerAddress).HasColumnName("CustomerAddress").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(CustomerAddressConverter);
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

                builder.Property(b => b.Active).HasDefaultValue(false);

                builder.Property(b => b.NextScheduledTransaction).HasColumnType("date").IsRequired(false);
                builder.Property(b => b.PausedFrom).HasColumnType("date").IsRequired(false);
                builder.Property(b => b.PausedTo).HasColumnType("date").IsRequired(false);
            }
        }

        internal class BillingDealHistoryConfiguration : IEntityTypeConfiguration<BillingDealHistory>
        {
            public void Configure(EntityTypeBuilder<BillingDealHistory> builder)
            {
                builder.ToTable("BillingDealHistory");

                builder.HasKey(b => b.BillingDealHistoryID);
                builder.Property(b => b.BillingDealHistoryID).ValueGeneratedNever();

                builder.Property(b => b.BillingDealID).IsRequired();

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
                    s.Property(p => p.ExternalConsumerCode).HasColumnName("ExternalConsumerCode").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                    s.Property(p => p.ConsumerAddress).HasColumnName("CustomerAddress").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(CustomerAddressConverter);
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

                //TODO: remove after migrating to PaymentDetails
                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter).HasColumnName("CardExpiration");
                    s.Property(p => p.CardNumber).HasColumnName("CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                    s.Ignore(p => p.CardBin);
                    s.Property(p => p.CardVendor).HasColumnName("CardVendor").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardBrand).HasColumnName("CardBrand").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.Solek).HasColumnName("Solek").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                });

                builder.Property(p => p.PaymentDetails).HasColumnName("PaymentDetails").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true)
                    .HasConversion(PaymentDetailsConverter)
                    .Metadata.SetValueComparer(PaymentDetailsComparer);

                builder.Property(b => b.InstallmentPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InitialPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalDiscount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(p => p.TransactionType).HasColumnName("TransactionType").HasColumnType("smallint").IsRequired(false);
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
                    s.Property(p => p.ExternalConsumerCode).HasColumnName("ExternalConsumerCode").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(ItemsConverter)
                        .Metadata.SetValueComparer(ItemsComparer);
                    s.Property(p => p.ConsumerAddress).HasColumnName("CustomerAddress").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(CustomerAddressConverter);
                });

                builder.OwnsOne(b => b.InvoiceDetails, s =>
                {
                    s.Property(p => p.InvoiceType).HasColumnName("InvoiceType");
                    s.Property(p => p.InvoiceSubject).HasColumnName("InvoiceSubject").IsRequired(false).IsUnicode(true);
                    s.Property(p => p.SendCCTo).HasColumnName("SendCCTo").IsRequired(false).IsUnicode(true).HasConversion(StringsArrayConverter)
                        .Metadata.SetValueComparer(StringArrayComparer);
                });

                builder.OwnsOne(b => b.PinPadDetails, s =>
                {
                    s.Property(p => p.TerminalID).HasColumnName("PinPadTerminalID").IsRequired(false).IsUnicode(false).HasMaxLength(16);
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

        internal class FutureBillingConfiguration : IEntityTypeConfiguration<FutureBilling>
        {
            public void Configure(EntityTypeBuilder<FutureBilling> builder)
            {
                builder.ToTable("vFutureBillings", t => t.ExcludeFromMigrations());

                builder.HasKey(b => new { b.BillingDealID, b.CurrentDeal });

                builder.Property(p => p.TerminalID).HasColumnName("TerminalID");
                builder.Property(p => p.MerchantID).HasColumnName("MerchantID");

                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter).HasColumnName("CardExpiration");
                    s.Property(p => p.CardNumber).HasColumnName("CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(p => p.CardOwnerNationalID);
                    s.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                    s.Ignore(p => p.CardBin);
                    s.Ignore(p => p.CardVendor);
                    s.Ignore(b => b.CardReaderInput);
                    s.Ignore(b => b.CardBrand);
                    s.Ignore(b => b.Solek);
                });

                builder.Property(b => b.TransactionAmount).HasColumnType("decimal(19,4)").HasColumnName("TransactionAmount");

                //builder.Property(b => b.Active).HasColumnName("Active");

                builder.Property(b => b.NextScheduledTransaction).HasColumnName("NextScheduledTransaction");
                builder.Property(b => b.FutureScheduledTransaction).HasColumnName("FutureScheduledTransaction");
                builder.Property(b => b.PausedFrom).HasColumnName("PausedFrom");
                builder.Property(b => b.PausedTo).HasColumnName("PausedTo");
            }
        }
    }
}
