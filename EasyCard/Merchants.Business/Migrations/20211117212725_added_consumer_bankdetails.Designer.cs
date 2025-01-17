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
    [Migration("20211117212725_added_consumer_bankdetails")]
    partial class added_consumer_bankdetails
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .UseIdentityColumns()
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.0");

            modelBuilder.Entity("Merchants.Business.Entities.Billing.Consumer", b =>
                {
                    b.Property<Guid>("ConsumerID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Active")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(true);

                    b.Property<string>("BankDetails")
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BillingDesktopRefNumber")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ConsumerAddress")
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ConsumerEmail")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ConsumerName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ConsumerNationalID")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ConsumerNote")
                        .HasMaxLength(512)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(512)");

                    b.Property<string>("ConsumerPhone")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ConsumerSecondPhone")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("ExternalReference")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OperationDoneBy")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid?>("OperationDoneByID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Origin")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("SourceIP")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<Guid>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ConsumerID");

                    b.HasIndex("TerminalID");

                    b.HasIndex("TerminalID", "ConsumerID");

                    b.HasIndex("TerminalID", "ExternalReference");

                    b.ToTable("Consumer");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Billing.CurrencyRate", b =>
                {
                    b.Property<long>("CurrencyRateID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

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

                    b.Property<string>("BillingDesktopRefNumber")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<short>("Currency")
                        .HasColumnType("smallint");

                    b.Property<string>("ExternalReference")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ItemName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("OperationDoneBy")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid?>("OperationDoneByID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(19,4)");

                    b.Property<string>("SourceIP")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ItemID");

                    b.HasIndex("MerchantID");

                    b.ToTable("Item");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Integration.ShvaTerminal", b =>
                {
                    b.Property<string>("MerchantNumber")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("Password")
                        .HasMaxLength(64)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(64)");

                    b.Property<string>("UserName")
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.HasKey("MerchantNumber");

                    b.ToTable("ShvaTerminal");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Feature", b =>
                {
                    b.Property<short>("FeatureID")
                        .HasColumnType("smallint");

                    b.Property<string>("DescriptionEN")
                        .HasMaxLength(1024)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("DescriptionHE")
                        .HasMaxLength(1024)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("NameEN")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("NameHE")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<decimal?>("Price")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("decimal(19,4)")
                        .HasDefaultValue(0m);

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("FeatureID");

                    b.ToTable("Feature");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Impersonation", b =>
                {
                    b.Property<Guid>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserId");

                    b.ToTable("Impersonation");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Merchant", b =>
                {
                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("BusinessID")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("BusinessName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("ContactPerson")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("MarketingName")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

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
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<Guid?>("MerchantID")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<short>("OperationCode")
                        .HasMaxLength(30)
                        .IsUnicode(false)
                        .HasColumnType("smallint");

                    b.Property<DateTime?>("OperationDate")
                        .IsRequired()
                        .HasColumnType("datetime2");

                    b.Property<string>("OperationDescription")
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OperationDoneBy")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid?>("OperationDoneByID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ReasonForChange")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("SourceIP")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<Guid?>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("MerchantHistoryID");

                    b.HasIndex("MerchantID");

                    b.HasIndex("TerminalID");

                    b.ToTable("MerchantHistory");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Plan", b =>
                {
                    b.Property<long>("PlanID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<decimal>("Price")
                        .HasColumnType("decimal(18,2)");

                    b.Property<long>("TerminalTemplateID")
                        .HasColumnType("bigint");

                    b.Property<string>("Title")
                        .HasColumnType("nvarchar(max)");

                    b.Property<byte[]>("UpdateTimestamp")
                        .HasColumnType("varbinary(max)");

                    b.HasKey("PlanID");

                    b.HasIndex("TerminalTemplateID");

                    b.ToTable("Plans");
                });

            modelBuilder.Entity("Merchants.Business.Entities.System.SystemSettings", b =>
                {
                    b.Property<int>("SystemSettingsID")
                        .HasColumnType("int");

                    b.Property<string>("BillingSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("BillingSettings");

                    b.Property<string>("CheckoutSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CheckoutSettings");

                    b.Property<string>("InvoiceSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("InvoiceSettings");

                    b.Property<string>("PaymentRequestSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("PaymentRequestSettings");

                    b.Property<string>("Settings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Settings");

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

                    b.Property<string>("AggregatorTerminalReference")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("BankDetails")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("BankDetails");

                    b.Property<string>("BillingSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("BillingSettings");

                    b.Property<string>("CheckoutSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CheckoutSettings");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("EnabledFeatures")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<string>("InvoiceSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("InvoiceSettings");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("PaymentRequestSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("PaymentRequestSettings");

                    b.Property<string>("ProcessorTerminalReference")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Settings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Settings");

                    b.Property<byte[]>("SharedApiKey")
                        .HasMaxLength(64)
                        .HasColumnType("varbinary(64)");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<long?>("TerminalTemplateID")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTime?>("Updated")
                        .HasColumnType("datetime2");

                    b.HasKey("TerminalID");

                    b.HasIndex("MerchantID");

                    b.ToTable("Terminal");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalExternalSystem", b =>
                {
                    b.Property<long>("TerminalExternalSystemID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<long>("ExternalSystemID")
                        .HasColumnType("bigint");

                    b.Property<string>("Settings")
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TerminalExternalSystemID");

                    b.HasIndex("TerminalID");

                    b.ToTable("TerminalExternalSystem");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalTemplate", b =>
                {
                    b.Property<long>("TerminalTemplateID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("BillingSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("BillingSettings");

                    b.Property<string>("CheckoutSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("CheckoutSettings");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("EnabledFeatures")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<string>("InvoiceSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("InvoiceSettings");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("PaymentRequestSettings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("PaymentRequestSettings");

                    b.Property<string>("Settings")
                        .IsUnicode(false)
                        .HasColumnType("nvarchar(max)")
                        .HasColumnName("Settings");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TerminalTemplateID");

                    b.ToTable("TerminalTemplate");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalTemplateExternalSystem", b =>
                {
                    b.Property<long>("TerminalTemplateExternalSystemID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<long>("ExternalSystemID")
                        .HasColumnType("bigint");

                    b.Property<string>("Settings")
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("TerminalTemplateID")
                        .HasColumnType("bigint");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TerminalTemplateExternalSystemID");

                    b.HasIndex("TerminalTemplateID");

                    b.ToTable("TerminalTemplateExternalSystem");
                });

            modelBuilder.Entity("Merchants.Business.Entities.User.UserTerminalMapping", b =>
                {
                    b.Property<long>("UserTerminalMappingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .UseIdentityColumn();

                    b.Property<string>("DisplayName")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Email")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("OperationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OperationDoneBy")
                        .HasMaxLength(50)
                        .IsUnicode(true)
                        .HasColumnType("nvarchar(50)");

                    b.Property<Guid?>("OperationDoneByID")
                        .HasMaxLength(50)
                        .IsUnicode(false)
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Roles")
                        .IsUnicode(false)
                        .HasColumnType("varchar(max)");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

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

                    b.Navigation("Merchant");
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

                    b.Navigation("Merchant");

                    b.Navigation("Terminal");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.Plan", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.TerminalTemplate", "TerminalTemplate")
                        .WithMany()
                        .HasForeignKey("TerminalTemplateID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("TerminalTemplate");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.Terminal", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Merchant.Merchant", "Merchant")
                        .WithMany()
                        .HasForeignKey("MerchantID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Merchant");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalExternalSystem", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany("Integrations")
                        .HasForeignKey("TerminalID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Terminal");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalTemplateExternalSystem", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.TerminalTemplate", "TerminalTemplate")
                        .WithMany("Integrations")
                        .HasForeignKey("TerminalTemplateID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("TerminalTemplate");
                });

            modelBuilder.Entity("Merchants.Business.Entities.User.UserTerminalMapping", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany()
                        .HasForeignKey("TerminalID");

                    b.Navigation("Terminal");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.Terminal", b =>
                {
                    b.Navigation("Integrations");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalTemplate", b =>
                {
                    b.Navigation("Integrations");
                });
#pragma warning restore 612, 618
        }
    }
}
