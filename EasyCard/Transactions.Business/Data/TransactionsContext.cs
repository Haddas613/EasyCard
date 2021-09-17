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
    public partial class TransactionsContext : DbContext
    {
        public static readonly LoggerFactory DbCommandConsoleLoggerFactory
            = new LoggerFactory(new[]
            {
                new DebugLoggerProvider()
            });

        private static readonly ValueConverter CardExpirationConverter = new ValueConverter<CardExpiration, DateTime?>(
            v => v.ToDate(),
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

        private static readonly ValueConverter SettingsJObjectConverter = new ValueConverter<JObject, string>(
           v => v.ToString(Formatting.None),
           v => JObject.Parse(v));

        private static readonly ValueComparer SettingsJObjectComparer = new ValueComparer<JObject>(
           (s1, s2) => s1.ToString(Formatting.None).GetHashCode() == s2.ToString(Formatting.None).GetHashCode(),
           v => v.ToString(Formatting.None).GetHashCode(),
           v => JObject.Parse(v.ToString(Formatting.None)));

        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<CreditCardTokenDetails> CreditCardTokenDetails { get; set; }

        public DbSet<TransactionHistory> TransactionHistories { get; set; }

        public DbSet<BillingDealHistory> BillingDealHistories { get; set; }

        public DbSet<BillingDeal> BillingDeals { get; set; }

        public DbSet<Invoice> Invoices { get; set; }

        public DbSet<PaymentRequest> PaymentRequests { get; set; }

        public DbSet<PaymentRequestHistory> PaymentRequestHistories { get; set; }

        public DbSet<FutureBilling> FutureBillings { get; set; }

        public DbSet<MasavFile> MasavFiles { get; set; }

        public DbSet<MasavFileRow> MasavFileRows { get; set; }

        public DbSet<NayaxTransactionsParameters> NayaxTransactionsParameters { get; set; }

        private readonly ClaimsPrincipal user;

        public TransactionsContext(DbContextOptions<TransactionsContext> options, IHttpContextAccessorWrapper httpContextAccessor)
            : base(options)
        {
            this.user = httpContextAccessor.GetUser();
        }

        // NOTE: use this for debugging purposes to analyse sql query performance
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLoggerFactory(DbCommandConsoleLoggerFactory);

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
            modelBuilder.ApplyConfiguration(new MasavFileConfiguration());
            modelBuilder.ApplyConfiguration(new MasavFileRowConfiguration());
            modelBuilder.ApplyConfiguration(new NayaxTransactionsParametersConfiguration());

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

                //TODO: remove
                builder.Property(typeof(string), "CardExpiration").HasMaxLength(5).IsUnicode(false).IsRequired(false);

                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasConversion(CardExpirationConverter)
                        .HasColumnName("CardExpirationDate")
                        .HasColumnType("date");

                    s.Property(p => p.CardNumber).HasColumnName("CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                    s.Property(p => p.CardBin).HasColumnName("CardBin").IsRequired(false).HasMaxLength(10).IsUnicode(false);
                    s.Property(p => p.CardVendor).HasColumnName("CardVendor").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardBrand).HasColumnName("CardBrand").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                    s.Ignore(b => b.Solek);
                });

                builder.OwnsOne(b => b.BankTransferDetails, s =>
                {
                    s.Property(p => p.Bank).IsRequired(false).HasColumnName("BankTransferBank");
                    s.Property(p => p.BankAccount).IsRequired(false).HasColumnName("BankTransferBankAccount").HasMaxLength(50);
                    s.Property(p => p.BankBranch).IsRequired(false).HasColumnName("BankTransferBankBranch");
                    s.Property(p => p.DueDate).IsRequired(false).HasColumnName("BankTransferDueDate");
                    s.Ignore(p => p.PaymentType);
                    s.Property(p => p.Reference).IsRequired(false).HasColumnName("BankTransferReference").HasMaxLength(50);
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
                    s.Property(p => p.ShvaShovarNumber).HasColumnName("ShvaShovarNumber").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ManuallyTransmitted).HasColumnName("ManuallyTransmitted");
                    s.Property(p => p.ShvaTerminalID).HasColumnName("ShvaTerminalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ShvaTransmissionNumber).HasColumnName("ShvaTransmissionNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ShvaAuthNum).HasColumnName("ShvaAuthNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.TransmissionDate).HasColumnName("ShvaTransmissionDate").IsRequired(false);
                    s.Property(p => p.Solek).HasColumnName("Solek").IsRequired(false);
                    s.Property(p => p.TranRecord).HasColumnName("ShvaTranRecord").HasMaxLength(600).IsUnicode(false).IsRequired(false);
                    s.Property(p => p.EmvSoftVersion).HasColumnName("EmvSoftVersion").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CompRetailerNum).HasColumnName("CompRetailerNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.TelToGetAuthNum).HasColumnName("TelToGetAuthNum").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                });

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerExternalReference).HasColumnName("ConsumerExternalReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ConsumerName).HasColumnName("ConsumerName").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerNationalID).HasColumnName("ConsumerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
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
                builder.Property(p => p.PinPadTransactionID).HasColumnName("PinPadTransactionID").IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.Extension).IsRequired(false).IsUnicode(true).HasConversion(SettingsJObjectConverter)
                    .Metadata.SetValueComparer(SettingsJObjectComparer);

                builder.HasIndex(d => d.PinPadTransactionID);
                builder.HasIndex(b => new { b.TerminalID, b.PaymentTypeEnum, b.MasavFileID });
                builder.HasIndex(b => new { b.MerchantID, b.TerminalID });
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

                //TODO: remove
                builder.Property(typeof(string), "CardExpirationOld").HasMaxLength(5).IsUnicode(false).IsRequired(false).HasColumnName("CardExpiration");

                builder.Property(b => b.CardExpiration).IsRequired(false).HasConversion(CardExpirationConverter)
                    .HasColumnName("CardExpirationDate")
                    .HasColumnType("date");

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

                builder.Property(p => p.ExpirationDate).HasColumnName("CardExpirationDate").HasColumnType("date");

                builder.Property(p => p.ReplacementOfTokenID).IsRequired(false);
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

                builder.Property(p => p.CreditCardToken).IsRequired(false).HasColumnName("CreditCardToken");

                //TODO: remove
                builder.Property(typeof(string), "CardExpiration").HasMaxLength(5).IsUnicode(false).IsRequired(false);

                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasConversion(CardExpirationConverter)
                        .HasColumnName("CardExpirationDate")
                        .HasColumnType("date");

                    s.Property(p => p.CardNumber).HasColumnName("CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerNationalID).HasColumnName("CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardOwnerName).HasColumnName("CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                    s.Property(p => p.CardBin).HasColumnName("CardBin").IsRequired(false).HasMaxLength(10).IsUnicode(false);
                    s.Property(p => p.CardVendor).HasColumnName("CardVendor").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.CardBrand).HasColumnName("CardBrand").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.Solek).HasColumnName("Solek").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Ignore(b => b.CardReaderInput);
                });

                builder.OwnsOne(b => b.BankDetails, s =>
                {
                    s.Property(p => p.Bank).IsRequired(false).HasColumnName("Bank");
                    s.Property(p => p.BankAccount).IsRequired(false).HasColumnName("BankAccount").HasMaxLength(50);
                    s.Property(p => p.BankBranch).IsRequired(false).HasColumnName("BankBranch");
                    s.Ignore(p => p.PaymentType);
                });

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerID).HasColumnName("ConsumerID");
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerExternalReference).HasColumnName("ConsumerExternalReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ConsumerName).HasColumnName("ConsumerName").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerNationalID).HasColumnName("ConsumerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
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
                builder.Property(b => b.PaymentType).IsRequired();

                builder.Property(b => b.LastErrorCorrelationID).IsRequired(false).HasMaxLength(50).IsUnicode(false);
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
                    s.Property(p => p.ConsumerExternalReference).HasColumnName("ConsumerExternalReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ConsumerName).HasColumnName("ConsumerName").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerNationalID).HasColumnName("ConsumerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
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

                // ToBeRemoved
                builder.Property(typeof(string), "CardOwnerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                builder.Property(typeof(string), "CardOwnerName").IsRequired(false).HasMaxLength(100).IsUnicode(true);
                builder.Property(typeof(string), "CardExpiration").IsRequired(false).HasMaxLength(5).IsUnicode(false);
                builder.Property(typeof(string), "CardNumber").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                builder.Property(typeof(string), "CardVendor").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                builder.Property(typeof(string), "CardBrand").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                builder.Property(typeof(string), "Solek").IsRequired(false).HasMaxLength(20).IsUnicode(false);

                builder.Property(b => b.CopyDonwnloadUrl).IsRequired(false).IsUnicode(false);
                builder.Property(b => b.DownloadUrl).IsRequired(false).IsUnicode(false);

                builder.Property(p => p.PaymentDetails).HasColumnName("PaymentDetails").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true)
                    .HasConversion(PaymentDetailsConverter)
                    .Metadata.SetValueComparer(PaymentDetailsComparer);

                builder.Property(b => b.InstallmentPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.InitialPaymentAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(b => b.TotalDiscount).HasColumnType("decimal(19,4)").IsRequired();
                builder.Property(p => p.TransactionType).HasColumnName("TransactionType").HasColumnType("smallint").IsRequired(false);

                builder.Property(b => b.ExternalSystemData).IsRequired(false).IsUnicode(true).HasConversion(SettingsJObjectConverter)
                    .Metadata.SetValueComparer(SettingsJObjectComparer);

                builder.Property(b => b.Extension).IsRequired(false).IsUnicode(true).HasConversion(SettingsJObjectConverter)
                    .Metadata.SetValueComparer(SettingsJObjectComparer);
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
                    s.Property(p => p.ConsumerExternalReference).HasColumnName("ConsumerExternalReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.DealReference).HasColumnName("DealReference").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone").IsRequired(false).HasMaxLength(20).IsUnicode(false);
                    s.Property(p => p.ConsumerName).HasColumnName("ConsumerName").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                    s.Property(p => p.ConsumerNationalID).HasColumnName("ConsumerNationalID").IsRequired(false).HasMaxLength(20).IsUnicode(false);
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

                //TODO: remove
                builder.Property(typeof(string), "CardExpiration").HasMaxLength(5).IsUnicode(false).IsRequired(false);

                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    s.Property(p => p.CardExpiration).IsRequired(false).HasConversion(CardExpirationConverter)
                        .HasColumnName("CardExpirationDate")
                        .HasColumnType("date");

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

        internal class NayaxTransactionsParametersConfiguration : IEntityTypeConfiguration<NayaxTransactionsParameters>
        {
            public void Configure(EntityTypeBuilder<NayaxTransactionsParameters> builder)
            {
                builder.ToTable("NayaxTransactionsParameters");

                builder.HasKey(b => b.NayaxTransactionsParametersID);
                builder.Property(b => b.NayaxTransactionsParametersID).ValueGeneratedNever();

                builder.Property(p => p.PinPadTransactionID).HasColumnName("PinPadTransactionID").IsRequired(false).HasMaxLength(50).IsUnicode(false);
                builder.Property(p => p.TranRecord).HasColumnName("ShvaTranRecord").HasMaxLength(600).IsUnicode(false).IsRequired(false);

                builder.HasIndex(d => d.PinPadTransactionID).IsUnique();
            }
        }

        internal class MasavFileConfiguration : IEntityTypeConfiguration<MasavFile>
        {
            public void Configure(EntityTypeBuilder<MasavFile> builder)
            {
                builder.ToTable("MasavFile");

                builder.HasKey(b => b.MasavFileID);

                builder.Property(b => b.MasavFileDate).HasColumnType("date");
                builder.Property(b => b.PayedDate);
                builder.Property(b => b.TotalAmount).HasColumnType("decimal(19,4)").HasColumnName("TotalAmount");
                builder.Property(b => b.StorageReference).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
                builder.Property(b => b.InstituteNumber);
                builder.Property(b => b.InstituteName).IsRequired(false).HasMaxLength(250).IsUnicode(true);
                builder.Property(b => b.SendingInstitute);
                builder.Property(b => b.Currency);
                builder.Property(b => b.TerminalID);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.HasIndex(d => d.MasavFileDate);
                builder.HasIndex(d => d.TerminalID);
            }
        }

        internal class MasavFileRowConfiguration : IEntityTypeConfiguration<MasavFileRow>
        {
            public void Configure(EntityTypeBuilder<MasavFileRow> builder)
            {
                builder.ToTable("MasavFileRow");

                builder.HasKey(b => b.MasavFileRowID);

                builder.Property(b => b.MasavFileID);
                builder.Property(b => b.PaymentTransactionID);
                builder.Property(b => b.ConsumerID);
                builder.Property(b => b.ConsumerName).IsRequired(false).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.Bankcode);
                builder.Property(b => b.BranchNumber);
                builder.Property(b => b.AccountNumber);
                builder.Property(b => b.NationalID);

                builder.Property(b => b.Amount).HasColumnType("decimal(19,4)");
                builder.Property(b => b.IsPayed);
                builder.Property(b => b.SmsSent);
                builder.Property(b => b.SmsSentDate);
            }
        }
    }
}
