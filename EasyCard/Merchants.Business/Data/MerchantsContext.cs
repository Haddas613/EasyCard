using Merchants.Business.Entities.Billing;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.System;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Merchants.Shared.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Business.Security;
using Shared.Helpers.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Merchants.Business.Data
{
    public class MerchantsContext : DbContext
    {
        public static readonly LoggerFactory DbCommandConsoleLoggerFactory
            = new LoggerFactory(new[]
            {
                new DebugLoggerProvider()
            });

        private static readonly ValueConverter SettingsJObjectConverter = new ValueConverter<JObject, string>(
           v => v.ToString(Formatting.None),
           v => JObject.Parse(v));

        private static readonly ValueConverter StringArrayConverter = new ValueConverter<IEnumerable<string>, string>(
            v => string.Join(",", v),
            v => v != null ? v.Split(new string[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries) : null);

        private static readonly ValueComparer StringArrayComparer = new ValueComparer<IEnumerable<string>>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => (IEnumerable<string>)c.ToHashSet());

        // terminal settings
        private static readonly ValueConverter TerminalSettingsConverter = new ValueConverter<TerminalSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<TerminalSettings>(v));

        private static readonly ValueConverter TerminalInvoiceSettingsConverter = new ValueConverter<TerminalInvoiceSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<TerminalInvoiceSettings>(v));

        private static readonly ValueConverter TerminalPaymentRequestSettingsConverter = new ValueConverter<TerminalPaymentRequestSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<TerminalPaymentRequestSettings>(v));

        private static readonly ValueConverter TerminalCheckoutSettingsConverter = new ValueConverter<TerminalCheckoutSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<TerminalCheckoutSettings>(v));

        private static readonly ValueConverter TerminalBillingSettingsConverter = new ValueConverter<TerminalBillingSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<TerminalBillingSettings>(v));

        // syste settings
        private static readonly ValueConverter SystemSettingsConverter = new ValueConverter<SystemGlobalSettings, string>(
        v => JsonConvert.SerializeObject(v),
        v => JsonConvert.DeserializeObject<SystemGlobalSettings>(v));

        private static readonly ValueConverter SystemInvoiceSettingsConverter = new ValueConverter<SystemInvoiceSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<SystemInvoiceSettings>(v));

        private static readonly ValueConverter SystemPaymentRequestSettingsConverter = new ValueConverter<SystemPaymentRequestSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<SystemPaymentRequestSettings>(v));

        private static readonly ValueConverter SystemCheckoutSettingsConverter = new ValueConverter<SystemCheckoutSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<SystemCheckoutSettings>(v));

        private static readonly ValueConverter SystemBillingSettingsConverter = new ValueConverter<SystemBillingSettings, string>(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<SystemBillingSettings>(v));

        public DbSet<Merchant> Merchants { get; set; }

        public DbSet<Feature> Features { get; set; }

        public DbSet<Terminal> Terminals { get; set; }

        public DbSet<TerminalExternalSystem> TerminalExternalSystems { get; set; }

        public DbSet<UserTerminalMapping> UserTerminalMappings { get; set; }

        public DbSet<MerchantHistory> MerchantHistories { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Consumer> Consumers { get; set; }

        public DbSet<CurrencyRate> CurrencyRates { get; set; }

        public DbSet<SystemSettings> SystemSettings { get; set; }

        private readonly ClaimsPrincipal user;

        public MerchantsContext(DbContextOptions<MerchantsContext> options, IHttpContextAccessorWrapper httpContextAccessor)
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
            modelBuilder.ApplyConfiguration(new MerchantConfiguration());
            modelBuilder.ApplyConfiguration(new TerminalConfiguration());
            modelBuilder.ApplyConfiguration(new FeatureConfiguration());
            modelBuilder.ApplyConfiguration(new TerminalExternalSystemConfiguration());
            modelBuilder.ApplyConfiguration(new UserTerminalMappingConfiguration());
            modelBuilder.ApplyConfiguration(new MerchantHistoryConfiguration());

            modelBuilder.ApplyConfiguration(new ItemConfiguration());
            modelBuilder.ApplyConfiguration(new ConsumerConfiguration());
            modelBuilder.ApplyConfiguration(new CurrencyRateConfiguration());

            modelBuilder.ApplyConfiguration(new SystemSettingsConfiguration());

            // NOTE: security filters moved to get methods

            //modelBuilder.Entity<Merchant>().HasQueryFilter(p => this.user.IsAdmin() || p.MerchantID == this.user.GetMerchantID());

            //modelBuilder.Entity<MerchantHistory>().HasQueryFilter(p => this.user.IsAdmin() || p.MerchantID == this.user.GetMerchantID());

            //modelBuilder.Entity<Terminal>().HasQueryFilter(p => this.user.IsAdmin() || ((user.IsTerminal() && user.GetTerminalID() == p.TerminalID) || p.MerchantID == user.GetMerchantID()));

            //modelBuilder.Entity<TerminalExternalSystem>().HasQueryFilter(p => this.user.IsAdmin() || ((user.IsTerminal() && user.GetTerminalID() == p.TerminalID) || p.Terminal.MerchantID == user.GetMerchantID()));

            //modelBuilder.Entity<UserTerminalMapping>().HasQueryFilter(p => this.user.IsAdmin() || ((user.IsTerminal() && user.GetTerminalID() == p.TerminalID) || p.Terminal.MerchantID == user.GetMerchantID()));

            modelBuilder.Entity<Item>()
                .HasQueryFilter(p => p.Active && (this.user.IsAdmin() || p.Merchant.MerchantID == this.user.GetMerchantID()));

            modelBuilder.Entity<Consumer>()
                .HasQueryFilter(p => p.Active && (this.user.IsAdmin() || p.MerchantID == this.user.GetMerchantID()));

            var cascadeFKs = modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetForeignKeys())
                .Where(fk => !fk.IsOwnership && fk.DeleteBehavior == DeleteBehavior.Cascade);

            foreach (var fk in cascadeFKs)
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }

            base.OnModelCreating(modelBuilder);
        }

        internal class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
        {
            public void Configure(EntityTypeBuilder<Merchant> builder)
            {
                builder.ToTable("Merchant");

                builder.HasKey(b => b.MerchantID);
                builder.Property(b => b.MerchantID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.BusinessName).IsRequired(true).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.MarketingName).IsRequired(false).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.ContactPerson).IsRequired(false).HasMaxLength(50).IsUnicode(true);
            }
        }

        internal class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
        {
            public void Configure(EntityTypeBuilder<Terminal> builder)
            {
                builder.ToTable("Terminal");

                builder.HasKey(b => b.TerminalID);
                builder.Property(b => b.TerminalID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.Label).IsRequired(true).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.ActivityStartDate).IsRequired(false);

                builder.Property(p => p.Settings).HasColumnName("Settings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(TerminalSettingsConverter);

                builder.Property(p => p.BillingSettings).HasColumnName("BillingSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(TerminalBillingSettingsConverter);

                builder.Property(p => p.CheckoutSettings).HasColumnName("CheckoutSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(TerminalCheckoutSettingsConverter);

                builder.Property(p => p.PaymentRequestSettings).HasColumnName("PaymentRequestSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(TerminalPaymentRequestSettingsConverter);

                builder.Property(p => p.InvoiceSettings).HasColumnName("InvoiceSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(TerminalInvoiceSettingsConverter);

                builder.Property(b => b.SharedApiKey).IsRequired(false).HasMaxLength(64);
            }
        }

        internal class FeatureConfiguration : IEntityTypeConfiguration<Feature>
        {
            public void Configure(EntityTypeBuilder<Feature> builder)
            {
                builder.ToTable("Feature");

                builder.HasKey(b => b.FeatureID);
                builder.Property(b => b.FeatureID).ValueGeneratedOnAdd();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.FeatureCode).IsRequired(true).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.NameEN).IsRequired(false).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.NameHE).IsRequired(false).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.Price).HasColumnType("decimal(19,4)").HasDefaultValue(decimal.Zero).IsRequired(false);
            }
        }

        internal class TerminalExternalSystemConfiguration : IEntityTypeConfiguration<TerminalExternalSystem>
        {
            public void Configure(EntityTypeBuilder<TerminalExternalSystem> builder)
            {
                builder.ToTable("TerminalExternalSystem");

                builder.HasKey(b => b.TerminalExternalSystemID);
                builder.Property(b => b.TerminalExternalSystemID).ValueGeneratedOnAdd();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.Settings).IsRequired(false).IsUnicode(true).HasConversion(SettingsJObjectConverter);
            }
        }

        internal class UserTerminalMappingConfiguration : IEntityTypeConfiguration<UserTerminalMapping>
        {
            public void Configure(EntityTypeBuilder<UserTerminalMapping> builder)
            {
                builder.ToTable("UserTerminalMapping");

                builder.HasKey(b => b.UserTerminalMappingID);
                builder.Property(b => b.UserTerminalMappingID).ValueGeneratedOnAdd();

                builder.Property(b => b.OperationDoneBy).IsRequired(false).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);
                builder.Property(b => b.UserID).IsRequired(true);

               // builder.HasIndex(idx => new { idx.UserID, idx.TerminalID }).IsUnique(true);

                builder.Property(b => b.Roles).IsRequired(false).IsUnicode(false).HasConversion(StringArrayConverter)
                    .Metadata.SetValueComparer(StringArrayComparer);

                builder.Property(b => b.DisplayName).IsRequired(false).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.Email).IsRequired(false).HasMaxLength(50).IsUnicode(true);
            }
        }

        internal class MerchantHistoryConfiguration : IEntityTypeConfiguration<MerchantHistory>
        {
            public void Configure(EntityTypeBuilder<MerchantHistory> builder)
            {
                builder.ToTable("MerchantHistory");

                builder.HasKey(b => b.MerchantHistoryID);
                builder.Property(b => b.MerchantHistoryID).ValueGeneratedNever();

                builder.Property(b => b.MerchantID).IsRequired();

                builder.Property(b => b.OperationDate).IsRequired();

                builder.Property(b => b.OperationCode).IsRequired().HasMaxLength(30).IsUnicode(false);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.OperationDescription).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.ReasonForChange).IsRequired(false).HasMaxLength(50).IsUnicode(true);
            }
        }

        internal class ItemConfiguration : IEntityTypeConfiguration<Item>
        {
            public void Configure(EntityTypeBuilder<Item> builder)
            {
                builder.ToTable("Item");

                builder.HasKey(b => b.ItemID);
                builder.Property(b => b.ItemID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.Active).HasDefaultValue(true);

                builder.Property(b => b.ItemName).IsRequired(true).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.CorrelationId).IsRequired().HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.Price).HasColumnType("decimal(19,4)").IsRequired();
            }
        }

        internal class ConsumerConfiguration : IEntityTypeConfiguration<Consumer>
        {
            public void Configure(EntityTypeBuilder<Consumer> builder)
            {
                builder.ToTable("Consumer");

                builder.HasKey(b => b.ConsumerID);
                builder.Property(b => b.ConsumerID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.Active).HasDefaultValue(true);

                builder.Property(b => b.ConsumerName).IsRequired(true).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.ConsumerEmail).IsRequired(true).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.ConsumerPhone).IsRequired(true).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.ConsumerNationalID).IsRequired(false).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.CorrelationId).IsRequired().HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.ConsumerAddress).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
            }
        }

        internal class CurrencyRateConfiguration : IEntityTypeConfiguration<CurrencyRate>
        {
            public void Configure(EntityTypeBuilder<CurrencyRate> builder)
            {
                builder.ToTable("CurrencyRate");

                builder.HasKey(b => b.CurrencyRateID);
                builder.Property(b => b.CurrencyRateID).ValueGeneratedOnAdd();

                builder.Property(b => b.Date).IsRequired(false).HasColumnType("date");

                builder.Property(b => b.Rate).HasColumnType("decimal(19,4)").IsRequired(false);
            }
        }

        internal class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
        {
            public void Configure(EntityTypeBuilder<SystemSettings> builder)
            {
                builder.ToTable("SystemSettings");

                builder.HasKey(b => b.SystemSettingsID);
                builder.Property(b => b.SystemSettingsID).ValueGeneratedNever();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(p => p.Settings).HasColumnName("Settings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(SystemSettingsConverter);

                builder.Property(p => p.BillingSettings).HasColumnName("BillingSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(SystemBillingSettingsConverter);

                builder.Property(p => p.CheckoutSettings).HasColumnName("CheckoutSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(SystemCheckoutSettingsConverter);

                builder.Property(p => p.PaymentRequestSettings).HasColumnName("PaymentRequestSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(SystemPaymentRequestSettingsConverter);

                builder.Property(p => p.InvoiceSettings).HasColumnName("InvoiceSettings").IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(false).HasConversion(SystemInvoiceSettingsConverter);
            }
        }
    }
}
