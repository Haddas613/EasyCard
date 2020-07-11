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
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using System.Threading.Tasks;
using System.Security;
using System.Data;
using Dapper;
using Transactions.Shared.Enums;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Transactions.Business.Data
{
    public class TransactionsContext : DbContext
    {
        public static readonly LoggerFactory DbCommandConsoleLoggerFactory
          = new LoggerFactory(new[] {
              new DebugLoggerProvider ()
            });

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<CreditCardTokenDetails> CreditCardTokenDetails { get; set; }

        public DbSet<TransactionHistory> TransactionHistories { get; set; }

        private readonly ClaimsPrincipal user;

        private static readonly ValueConverter CardExpirationConverter = new ValueConverter<CardExpiration, string>(
            v => v.ToString(),
            v => CreditCardHelpers.ParseCardExpiration(v));

        private static readonly ValueConverter SettingsJObjectConverter = new ValueConverter<JObject, string>(
           v => v.ToString(Formatting.None),
           v => JObject.Parse(v));

        public TransactionsContext(DbContextOptions<TransactionsContext> options, IHttpContextAccessorWrapper httpContextAccessor)
            : base(options)
        {
            this.user = httpContextAccessor.GetUser();
        }

        // NOTE: use this for debugging purposes to analyse sql query performance
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //    => optionsBuilder
        //        .UseLoggerFactory(DbCommandConsoleLoggerFactory);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new CreditCardTokenDetailsConfiguration());
            modelBuilder.ApplyConfiguration(new TransactionHistoryConfiguration());

            // security filters

            modelBuilder.Entity<CreditCardTokenDetails>().HasQueryFilter(t => user.IsAdmin() || (t.Active && (user.IsTerminal() && t.TerminalID == user.GetTerminalID() || t.MerchantID == user.GetMerchantID())));

            modelBuilder.Entity<PaymentTransaction>().HasQueryFilter(t => user.IsAdmin() || ((user.IsTerminal() && t.TerminalID == user.GetTerminalID() || t.MerchantID == user.GetMerchantID())));

            modelBuilder.Entity<TransactionHistory>().HasQueryFilter(t => user.IsAdmin() || ((user.IsTerminal() && t.PaymentTransaction.TerminalID == user.GetTerminalID() || t.PaymentTransaction.MerchantID == user.GetMerchantID())));

            base.OnModelCreating(modelBuilder);
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

                var report = await connection.QueryAsync<TransmissionInfo>(query, new { NewStatus = TransactionStatusEnum.TransmissionInProgress, OldStatus = TransactionStatusEnum.CommitedByAggregator, TerminalID = terminalID, MerchantID = user.GetMerchantID(), TransactionIDs = transactionIDs, UpdatedDate = DateTime.UtcNow }, transaction: dbTransaction?.GetDbTransaction());

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

                builder.Property(p => p.CreditCardToken).HasColumnName("CreditCardToken").IsRequired(false).HasMaxLength(50).IsUnicode(false);

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
                    s.Property(p => p.Items).HasColumnName("Items").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true).HasConversion(SettingsJObjectConverter);


                });

                builder.Property(p => p.ConsumerIP).HasColumnName("ConsumerIP").IsRequired(false).HasMaxLength(32).IsUnicode(false);
                builder.Property(p => p.MerchantIP).HasColumnName("MerchantIP").IsRequired(false).HasMaxLength(32).IsUnicode(false);

                builder.Property(b => b.TransactionAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InstallmentPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InitialPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").IsRequired();

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);
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
    }
}
