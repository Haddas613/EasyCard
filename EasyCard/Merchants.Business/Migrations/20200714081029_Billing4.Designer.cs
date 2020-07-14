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
    [Migration("20200714081029_Billing4")]
    partial class Billing4
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
                        .HasColumnType("bit");

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

            modelBuilder.Entity("Merchants.Business.Entities.Billing.Item", b =>
                {
                    b.Property<Guid>("ItemID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

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

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("FeatureID");

                    b.HasIndex("TerminalID");

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

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.Terminal", b =>
                {
                    b.Property<Guid>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ActivityStartDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<Guid>("MerchantID")
                        .HasColumnType("uniqueidentifier");

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

                    b.Property<string>("ExternalProcessorReference")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<long>("ExternalSystemID")
                        .HasColumnType("bigint");

                    b.Property<string>("Settings")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(true);

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

                    b.Property<Guid>("TerminalID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<Guid>("UserID")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("UserTerminalMappingID");

                    b.HasIndex("TerminalID");

                    b.HasIndex("UserID", "TerminalID")
                        .IsUnique();

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

                    b.OwnsOne("Merchants.Business.Entities.Terminal.TerminalBillingSettings", "BillingSettings", b1 =>
                        {
                            b1.Property<Guid>("TerminalID")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("BillingNotificationsEmails")
                                .HasColumnName("BillingNotificationsEmails")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("TerminalID");

                            b1.ToTable("Terminal");

                            b1.WithOwner()
                                .HasForeignKey("TerminalID");
                        });

                    b.OwnsOne("Merchants.Business.Entities.Terminal.TerminalSettings", "Settings", b1 =>
                        {
                            b1.Property<Guid>("TerminalID")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<bool>("CvvRequired")
                                .ValueGeneratedOnAdd()
                                .HasColumnName("CvvRequired")
                                .HasColumnType("bit")
                                .HasDefaultValue(false);

                            b1.Property<bool>("EnableDeletionOfUntransmittedTransactions")
                                .ValueGeneratedOnAdd()
                                .HasColumnName("EnableDeletionOfUntransmittedTransactions")
                                .HasColumnType("bit")
                                .HasDefaultValue(false);

                            b1.Property<bool>("J2Allowed")
                                .ValueGeneratedOnAdd()
                                .HasColumnName("J2Allowed")
                                .HasColumnType("bit")
                                .HasDefaultValue(false);

                            b1.Property<bool>("J5Allowed")
                                .ValueGeneratedOnAdd()
                                .HasColumnName("J5Allowed")
                                .HasColumnType("bit")
                                .HasDefaultValue(false);

                            b1.Property<int?>("MaxCreditInstallments")
                                .HasColumnType("int");

                            b1.Property<int?>("MaxInstallments")
                                .HasColumnType("int");

                            b1.Property<int?>("MinCreditInstallments")
                                .HasColumnType("int");

                            b1.Property<int?>("MinInstallments")
                                .HasColumnType("int");

                            b1.Property<bool>("NationalIDRequired")
                                .ValueGeneratedOnAdd()
                                .HasColumnName("NationalIDRequired")
                                .HasColumnType("bit")
                                .HasDefaultValue(false);

                            b1.Property<string>("PaymentButtonSettings")
                                .HasColumnName("PaymentButtonSettings")
                                .HasColumnType("nvarchar(max)")
                                .IsUnicode(true);

                            b1.Property<string>("RedirectPageSettings")
                                .HasColumnName("RedirectPageSettings")
                                .HasColumnType("nvarchar(max)")
                                .IsUnicode(true);

                            b1.HasKey("TerminalID");

                            b1.ToTable("Terminal");

                            b1.WithOwner()
                                .HasForeignKey("TerminalID");
                        });
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.TerminalExternalSystem", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany("Integrations")
                        .HasForeignKey("TerminalID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });

            modelBuilder.Entity("Merchants.Business.Entities.User.UserTerminalMapping", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany()
                        .HasForeignKey("TerminalID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
