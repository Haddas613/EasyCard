using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Business.Security;
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

        private readonly ClaimsPrincipal user;

        public TransactionsContext(DbContextOptions<TransactionsContext> options, IHttpContextAccessorWrapper httpContextAccessor)
            : base(options)
        {
            this.user = httpContextAccessor.GetUser();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
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

                builder.Property(b => b.CreditCardNumber).IsRequired(true).HasMaxLength(16).IsUnicode(false);
                builder.Property(b => b.ExpirationDate).IsRequired(true).HasMaxLength(4).IsUnicode(false);

                builder.Property(b => b.Amount).HasColumnType("decimal(19,4)").IsRequired(false);
            }
        }
    }
}
