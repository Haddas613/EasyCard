﻿// <auto-generated />
using System;
using Merchants.Business.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Merchants.Business.Migrations
{
    [DbContext(typeof(MerchantsContext))]
    [Migration("20201231111959_TerminalTemplates_added")]
    partial class TerminalTemplates_added
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0-preview.6.20312.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Merchants.Business.Entities.Billing.Consumer", b =>
                {
                    b.Property<Guid>("ConsumerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("ConsumerAddress")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(true);

                    b.Property<string>("ConsumerEmail")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("ConsumerName")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("ConsumerNationalID")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("ConsumerPhone")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OperationDoneBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid?>("OperationDoneByID")
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("SourceIP")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<Guid>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ConsumerID");

                    b.ToTable("Consumer");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Billing.CurrencyRate", b =>
                {
                    b.Property<long>("CurrencyRateID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<short>("Currency")
                        .HasColumnType("smallint");

                    b.Property<DateTime?>("Date")
                        .HasColumnType("date");

                    b.Property<decimal?>("Rate")
                        .HasColumnType("decimal(19,4)");

                    b.HasKey("CurrencyRateID");

                    b.ToTable("CurrencyRate");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Billing.Item", b =>
                {
                    b.Property<Guid>("ItemID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<short>("Currency")
                        .HasColumnType("smallint");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OperationDoneBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid?>("OperationDoneByID")
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(19,4)");

                    b.Property<string>("SourceIP")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ItemID");

                    b.HasIndex("MerchantID");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Feature", b =>
                {
                    b.Property<long>("FeatureID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("FeatureCode")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("NameEN")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("NameHE")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<decimal?>("Price")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(19,4)")
                        .HasDefaultValue(0m);

                    b.Property<Guid?>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long?>("TerminalTemplateID")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("FeatureID");

                    b.HasIndex("TerminalID");

                    b.HasIndex("TerminalTemplateID");

                    b.ToTable("Feature");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Merchant", b =>
                {
                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BusinessID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BusinessName")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("ContactPerson")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("MarketingName")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("MerchantID");

                    b.ToTable("Merchant");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.MerchantHistory", b =>
                {
                    b.Property<Guid>("MerchantHistoryID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CorrelationId")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<Guid?>("MerchantID")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<short>("OperationCode")
                        .HasColumnType("smallint")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    b.Property<DateTime?>("OperationDate")
                        .IsRequired()
                        .HasColumnType("datetime2");

                    b.Property<string>("OperationDescription")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(true);

                    b.Property<string>("OperationDoneBy")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid?>("OperationDoneByID")
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("ReasonForChange")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("SourceIP")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<Guid?>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("MerchantHistoryID");

                    b.HasIndex("MerchantID");

                    b.HasIndex("TerminalID");

                    b.ToTable("MerchantHistory");
                });

            modelBuilder.Entity("Merchants.Business.Entities.System.SystemSettings", b =>
                {
                    b.Property<int>("SystemSettingsID")
                        .HasColumnType("int");

                    b.Property<string>("BillingSettings")
                        .HasColumnName("BillingSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("CheckoutSettings")
                        .HasColumnName("CheckoutSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("InvoiceSettings")
                        .HasColumnName("InvoiceSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("PaymentRequestSettings")
                        .HasColumnName("PaymentRequestSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Settings")
                        .HasColumnName("Settings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("SystemSettingsID");

                    b.ToTable("SystemSettings");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.Terminal", b =>
                {
                    b.Property<Guid>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ActivityStartDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("BillingSettings")
                        .HasColumnName("BillingSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("CheckoutSettings")
                        .HasColumnName("CheckoutSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("InvoiceSettings")
                        .HasColumnName("InvoiceSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PaymentRequestSettings")
                        .HasColumnName("PaymentRequestSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Settings")
                        .HasColumnName("Settings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<byte[]>("SharedApiKey")
                        .HasColumnType("varbinary(64)")
                        .HasMaxLength(64);

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TerminalID");

                    b.HasIndex("MerchantID");

                    b.ToTable("Terminal");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalExternalSystem", b =>
                {
                    b.Property<long>("TerminalExternalSystemID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<long>("ExternalSystemID")
                        .HasColumnType("bigint");

                    b.Property<string>("Settings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(true);

                    b.Property<Guid>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long?>("TerminalTemplateID")
                        .HasColumnType("bigint");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TerminalExternalSystemID");

                    b.HasIndex("TerminalID");

                    b.HasIndex("TerminalTemplateID");

                    b.ToTable("TerminalExternalSystem");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalTemplate", b =>
                {
                    b.Property<long>("TerminalTemplateID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("BillingSettings")
                        .HasColumnName("BillingSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("CheckoutSettings")
                        .HasColumnName("CheckoutSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("InvoiceSettings")
                        .HasColumnName("InvoiceSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("PaymentRequestSettings")
                        .HasColumnName("PaymentRequestSettings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<string>("Settings")
                        .HasColumnName("Settings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(false);

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TerminalTemplateID");

                    b.ToTable("TerminalTemplate");
                });

            modelBuilder.Entity("Merchants.Business.Entities.User.UserTerminalMapping", b =>
                {
                    b.Property<long>("UserTerminalMappingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("DisplayName")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("OperationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OperationDoneBy")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid?>("OperationDoneByID")
                        .HasColumnType("uniqueidentifier")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("Roles")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.Property<Guid?>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserTerminalMappingID");

                    b.HasIndex("TerminalID");

                    b.ToTable("UserTerminalMapping");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Billing.Item", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Merchant.Merchant", "Merchant")
                        .WithMany()
                        .HasForeignKey("MerchantID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Feature", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", null)
                        .WithMany("EnabledFeatures")
                        .HasForeignKey("TerminalID");

                    b.HasOne("Merchants.Business.Entities.Terminal.TerminalTemplate", null)
                        .WithMany("EnabledFeatures")
                        .HasForeignKey("TerminalTemplateID");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.MerchantHistory", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Merchant.Merchant", "Merchant")
                        .WithMany()
                        .HasForeignKey("MerchantID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany()
                        .HasForeignKey("TerminalID");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.Terminal", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Merchant.Merchant", "Merchant")
                        .WithMany()
                        .HasForeignKey("MerchantID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalExternalSystem", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany("Integrations")
                        .HasForeignKey("TerminalID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("Merchants.Business.Entities.Terminal.TerminalTemplate", null)
                        .WithMany("Integrations")
                        .HasForeignKey("TerminalTemplateID");
                });

            modelBuilder.Entity("Merchants.Business.Entities.User.UserTerminalMapping", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany()
                        .HasForeignKey("TerminalID");
                });
#pragma warning restore 612, 618
        }
    }
}
