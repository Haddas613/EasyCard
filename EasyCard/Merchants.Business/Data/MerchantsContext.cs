﻿using Merchants.Business.Entities.Billing;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
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

        public DbSet<Merchant> Merchants { get; set; }

        public DbSet<Feature> Features { get; set; }

        public DbSet<Terminal> Terminals { get; set; }

        public DbSet<TerminalExternalSystem> TerminalExternalSystems { get; set; }

        public DbSet<UserTerminalMapping> UserTerminalMappings { get; set; }

        public DbSet<MerchantHistory> MerchantHistories { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Consumer> Consumers { get; set; }

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

            // security filters

            modelBuilder.Entity<Merchant>().HasQueryFilter(p => this.user.IsAdmin() || p.MerchantID == this.user.GetMerchantID());

            modelBuilder.Entity<MerchantHistory>().HasQueryFilter(p => this.user.IsAdmin() || p.MerchantID == this.user.GetMerchantID());

            modelBuilder.Entity<Terminal>().HasQueryFilter(p => this.user.IsAdmin() || ((user.IsTerminal() && user.GetTerminalID() == p.TerminalID) || p.MerchantID == user.GetMerchantID()));

            modelBuilder.Entity<TerminalExternalSystem>().HasQueryFilter(p => this.user.IsAdmin() || ((user.IsTerminal() && user.GetTerminalID() == p.TerminalID) || p.Terminal.MerchantID == user.GetMerchantID()));

            modelBuilder.Entity<UserTerminalMapping>().HasQueryFilter(p => this.user.IsAdmin() || ((user.IsTerminal() && user.GetTerminalID() == p.TerminalID) || p.Terminal.MerchantID == user.GetMerchantID()));

            modelBuilder.Entity<Item>().HasQueryFilter(p => this.user.IsAdmin() || p.Merchant.MerchantID == this.user.GetMerchantID());

            modelBuilder.Entity<Consumer>().HasQueryFilter(p => this.user.IsAdmin() || p.MerchantID == this.user.GetMerchantID());

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

                builder.OwnsOne(b => b.Settings, s =>
                {
                    s.Property(p => p.CvvRequired).HasColumnName("CvvRequired").HasDefaultValue(false);
                    s.Property(p => p.J5Allowed).HasColumnName("J5Allowed").HasDefaultValue(false);
                    s.Property(p => p.J2Allowed).HasColumnName("J2Allowed").HasDefaultValue(false);
                    s.Property(p => p.CvvRequired).HasColumnName("CvvRequired").HasDefaultValue(false);
                    s.Property(p => p.EnableDeletionOfUntransmittedTransactions).HasColumnName("EnableDeletionOfUntransmittedTransactions").HasDefaultValue(false);
                    s.Property(p => p.NationalIDRequired).HasColumnName("NationalIDRequired").HasDefaultValue(false);
                    s.Property(p => p.PaymentButtonSettings).HasColumnName("PaymentButtonSettings").IsRequired(false).IsUnicode(true).HasConversion(SettingsJObjectConverter);
                    s.Property(p => p.RedirectPageSettings).HasColumnName("RedirectPageSettings").IsRequired(false).IsUnicode(true).HasConversion(SettingsJObjectConverter);
                });

                builder.OwnsOne(b => b.BillingSettings, s =>
                {
                    s.Property(p => p.BillingNotificationsEmails).HasColumnName("BillingNotificationsEmails").IsRequired(false);
                });
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

                builder.Property(b => b.ExternalProcessorReference).IsRequired(false).HasMaxLength(50).IsUnicode(false);
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

                builder.HasIndex(idx => new { idx.UserID, idx.TerminalID }).IsUnique(true);

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

                builder.Property(b => b.ConsumerName).IsRequired(true).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.ConsumerEmail).IsRequired(true).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.ConsumerPhone).IsRequired(true).HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.CorrelationId).IsRequired().HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.ConsumerAddress).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);
            }
        }
    }
}
