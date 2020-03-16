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

namespace Transactions.Business.Data
{
    public class TransactionsContext : DbContext
    {
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        public DbSet<CreditCardTokenDetails> CreditCardTokenDetails { get; set; }

        private readonly ClaimsPrincipal user;

        private static readonly ValueConverter CardExpirationConverter = new ValueConverter<CardExpiration, string>(
            v => v.ToString(),
            v => CreditCardHelpers.ParseCardExpiration(v));

        public TransactionsContext(DbContextOptions<TransactionsContext> options, IHttpContextAccessorWrapper httpContextAccessor)
            : base(options)
        {
            this.user = httpContextAccessor.GetUser();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
            modelBuilder.ApplyConfiguration(new CreditCardTokenDetailsConfiguration());

            modelBuilder.Entity<CreditCardTokenDetails>().HasQueryFilter(t => t.Active);

            base.OnModelCreating(modelBuilder);
        }

        internal class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
        {
            public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
            {
                builder.ToTable("PaymentTransaction");

                builder.HasKey(b => b.PaymentTransactionID);
                builder.Property(b => b.PaymentTransactionID).ValueGeneratedOnAdd();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                //builder.Property(b => b.CardNumber).IsRequired(true).HasMaxLength(16).IsUnicode(false);
                builder.OwnsOne(b => b.CreditCardDetails, s =>
                {
                    //TODO: rest of the fields
                    s.Property(p => p.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter).HasColumnName("CardExpiration");
                });

                builder.OwnsOne(b => b.ClearingHouseTransactionDetails, s =>
                {
                    s.Property(p => p.ClearingHouseTransactionID).HasColumnName("ClearingHouseTransactionID");
                });

                builder.OwnsOne(b => b.ShvaTransactionDetails, s =>
                {
                    s.Property(p => p.ShvaDealID).HasColumnName("ShvaDealID");
                    s.Property(p => p.ShvaShovarNumber).HasColumnName("ShvaShovarNumber");
                    s.Property(p => p.ManuallyTransmitted).HasColumnName("ManuallyTransmitted");
                    s.Property(p => p.ShvaTerminalID).HasColumnName("ShvaTerminalID");
                    s.Property(p => p.ShvaTransmissionNumber).HasColumnName("ShvaTransmissionNumber");
                });

                builder.OwnsOne(b => b.DealDetails, s =>
                {
                    s.Property(p => p.ConsumerEmail).HasColumnName("ConsumerEmail");
                    s.Property(p => p.ConsumerIP).HasColumnName("ConsumerIP");
                    s.Property(p => p.ConsumerPhone).HasColumnName("ConsumerPhone");
                    s.Property(p => p.DealDescription).HasColumnName("DealDescription");
                    s.Property(p => p.MerchantIP).HasColumnName("MerchantIP");
                });

                //builder.Property(b => b.CreditCardDetails.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter);

                //builder.Property(b => b.Amount).HasColumnType("decimal(19,4)").IsRequired(false);
            }
        }

        internal class CreditCardTokenDetailsConfiguration : IEntityTypeConfiguration<CreditCardTokenDetails>
        {
            public void Configure(EntityTypeBuilder<CreditCardTokenDetails> builder)
            {
                builder.ToTable("CreditCardTokenDetails");

                builder.HasKey(b => b.CreditCardTokenID);
                builder.Property(b => b.CreditCardTokenID).ValueGeneratedOnAdd();
                builder.Property(b => b.PublicKey).IsRequired(true).HasMaxLength(64).IsUnicode(false);
                builder.Property(b => b.Hash).IsRequired(true).HasMaxLength(256).IsUnicode(false);
                builder.Property(b => b.CardNumber).IsRequired(true).HasMaxLength(16).IsUnicode(false);
                builder.Property(b => b.CardExpiration).IsRequired(false).HasMaxLength(5).IsUnicode(false).HasConversion(CardExpirationConverter);
                builder.Property(b => b.CardVendor).IsRequired(true);
                builder.Property(b => b.CardOwnerNationalID).IsRequired(true);
                builder.Property(b => b.Active).HasDefaultValue(true);
            }
        }
    }
}
