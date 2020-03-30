using Merchants.Business.Entities.Integration;
using Merchants.Business.Entities.Merchant;
using Merchants.Business.Entities.Terminal;
using Merchants.Business.Entities.User;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Business.Security;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace Merchants.Business.Data
{
    public class MerchantsContext : DbContext
    {
        public DbSet<Merchant> Merchants { get; set; }

        public DbSet<Feature> Features { get; set; }

        public DbSet<Terminal> Terminals { get; set; }

        public DbSet<ExternalSystem> ExternalSystems { get; set; }

        public DbSet<TerminalExternalSystem> TerminalExternalSystems { get; set; }

        public DbSet<UserTerminalMapping> UserTerminalMappings { get; set; }

        public DbSet<MerchantHistory> MerchantHistories { get; set; }

        private readonly ClaimsPrincipal user;

        private static readonly ValueConverter SettingsJObjectConverter = new ValueConverter<JObject, string>(
           v => v.ToString(Formatting.None),
           v => JObject.Parse(v));

        public MerchantsContext(DbContextOptions<MerchantsContext> options, IHttpContextAccessorWrapper httpContextAccessor)
            : base(options)
        {
            this.user = httpContextAccessor.GetUser();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new MerchantConfiguration());
            modelBuilder.ApplyConfiguration(new TerminalConfiguration());
            modelBuilder.ApplyConfiguration(new FeatureConfiguration());
            modelBuilder.ApplyConfiguration(new ExternalSystemConfiguration());
            modelBuilder.ApplyConfiguration(new TerminalExternalSystemConfiguration());
            modelBuilder.ApplyConfiguration(new UserTerminalMappingConfiguration());
            modelBuilder.ApplyConfiguration(new MerchantHistoryConfiguration());

            // security filters

            //modelBuilder.Entity<Merchant>().HasQueryFilter(p => this.user.IsAdmin() || p.MerchantID == this.user.GetMerchantID());

            base.OnModelCreating(modelBuilder);
        }

        internal class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
        {
            public void Configure(EntityTypeBuilder<Merchant> builder)
            {
                builder.ToTable("Merchant");

                builder.HasKey(b => b.MerchantID);
                builder.Property(b => b.MerchantID).ValueGeneratedOnAdd();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.BusinessName).IsRequired(true).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.MarketingName).IsRequired(false).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.ContactPerson).IsRequired(false).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.Users).IsRequired(false).IsUnicode(false);
            }
        }

        internal class TerminalConfiguration : IEntityTypeConfiguration<Terminal>
        {
            public void Configure(EntityTypeBuilder<Terminal> builder)
            {
                builder.ToTable("Terminal");

                builder.HasKey(b => b.TerminalID);
                builder.Property(b => b.TerminalID).ValueGeneratedOnAdd();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.Label).IsRequired(true).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.ActivityStartDate).IsRequired(false);

                builder.OwnsOne(b => b.Settings, s =>
                {
                    s.Property(p => p.CvvRequired).HasDefaultValue(false);
                    s.Property(p => p.EnableDeletionOfUntransmittedTransactions).HasDefaultValue(false);
                    s.Property(p => p.NationalIDRequired).HasDefaultValue(false);
                    s.Property(p => p.PaymentButtonSettings).IsRequired(false).IsUnicode(true);
                    s.Property(p => p.RedirectPageSettings).IsRequired(false).IsUnicode(true);
                });

                builder.OwnsOne(b => b.BillingSettings, s =>
                {
                    s.Property(p => p.BillingNotificationsEmails).IsRequired(false);
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

        internal class ExternalSystemConfiguration : IEntityTypeConfiguration<ExternalSystem>
        {
            public void Configure(EntityTypeBuilder<ExternalSystem> builder)
            {
                builder.ToTable("ExternalSystem");

                builder.HasKey(b => b.ExternalSystemID);
                builder.Property(b => b.ExternalSystemID).ValueGeneratedOnAdd();

                builder.Property(p => p.UpdateTimestamp).IsRowVersion();

                builder.Property(b => b.Name).IsRequired(true).HasMaxLength(50).IsUnicode(true);
                builder.Property(b => b.InstanceTypeFullName).IsRequired(true).HasMaxLength(512).IsUnicode(false);
                builder.Property(b => b.Settings).IsRequired(true).IsUnicode(true);
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
                builder.Property(b => b.UserID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.HasIndex(idx => new { idx.UserID, idx.TerminalID }).IsUnique(true);
            }
        }

        internal class MerchantHistoryConfiguration : IEntityTypeConfiguration<MerchantHistory>
        {
            public void Configure(EntityTypeBuilder<MerchantHistory> builder)
            {
                builder.ToTable("MerchantHistory");

                builder.HasKey(b => b.MerchantHistoryID);
                builder.Property(b => b.MerchantHistoryID).ValueGeneratedOnAdd();

                builder.Property(b => b.MerchantID).IsRequired();

                builder.Property(b => b.OperationDate).IsRequired();

                builder.Property(b => b.OperationCode).IsRequired().HasMaxLength(30).IsUnicode(false);

                builder.Property(b => b.OperationDoneBy).IsRequired().HasMaxLength(50).IsUnicode(true);

                builder.Property(b => b.OperationDoneByID).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.OperationDescription).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);

                builder.Property(b => b.AdditionalDetails).IsRequired(false).HasColumnType("nvarchar(max)").IsUnicode(true);

                builder.Property(b => b.CorrelationId).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.SourceIP).IsRequired(false).HasMaxLength(50).IsUnicode(false);

                builder.Property(b => b.ReasonForChange).IsRequired(false).HasMaxLength(50).IsUnicode(true);
            }
        }
    }
}
