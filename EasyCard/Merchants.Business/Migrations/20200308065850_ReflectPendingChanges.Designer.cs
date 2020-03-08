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
    [Migration("20200308065850_ReflectPendingChanges")]
    partial class ReflectPendingChanges
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Merchants.Business.Entities.Integration.ExternalSystem", b =>
                {
                    b.Property<long>("ExternalSystemID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("Settings")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(true);

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("ExternalSystemID");

                    b.ToTable("ExternalSystem");
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

                    b.Property<long?>("TerminalID")
                        .HasColumnType("bigint");

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
                    b.Property<long>("MerchantID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

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

                    b.Property<string>("Users")
                        .HasColumnType("varchar(max)")
                        .IsUnicode(false);

                    b.HasKey("MerchantID");

                    b.ToTable("Merchant");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Merchant.MerchantHistory", b =>
                {
                    b.Property<long>("MerchantHistoryID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("AdditionalDetails")
                        .HasColumnType("nvarchar(max)")
                        .IsUnicode(true);

                    b.Property<string>("CorrelationId")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<long?>("MerchantID")
                        .IsRequired()
                        .HasColumnType("bigint");

                    b.Property<string>("OperationCode")
                        .IsRequired()
                        .HasColumnType("varchar(30)")
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

                    b.Property<string>("OperationDoneByID")
                        .HasColumnType("varchar(50)")
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

                    b.Property<long?>("TerminalID")
                        .HasColumnType("bigint");

                    b.HasKey("MerchantHistoryID");

                    b.HasIndex("MerchantID");

                    b.HasIndex("TerminalID");

                    b.ToTable("MerchantHistory");
                });

            modelBuilder.Entity("Merchants.Business.Entities.Terminal.Terminal", b =>
                {
                    b.Property<long>("TerminalID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime?>("ActivityStartDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<long>("MerchantID")
                        .HasColumnType("bigint");

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

                    b.Property<long>("TerminalID")
                        .HasColumnType("bigint");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("TerminalExternalSystemID");

                    b.HasIndex("ExternalSystemID");

                    b.HasIndex("TerminalID");

                    b.ToTable("TerminalExternalSystem");
                });

            modelBuilder.Entity("Merchants.Business.Entities.User.UserTerminalMapping", b =>
                {
                    b.Property<long>("UserTerminalMappingID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bigint")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("OperationDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("OperationDoneBy")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("OperationDoneByID")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<long>("TerminalID")
                        .HasColumnType("bigint");

                    b.Property<string>("UserID")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.HasKey("UserTerminalMappingID");

                    b.HasIndex("TerminalID");

                    b.HasIndex("UserID", "TerminalID")
                        .IsUnique()
                        .HasFilter("[UserID] IS NOT NULL");

                    b.ToTable("UserTerminalMapping");
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
                        .OnDelete(DeleteBehavior.Cascade)
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
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("Merchants.Business.Entities.Terminal.TerminalBillingSettings", "BillingSettings", b1 =>
                        {
                            b1.Property<long>("TerminalID")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<string>("BillingNotificationsEmails")
                                .HasColumnType("nvarchar(max)");

                            b1.HasKey("TerminalID");

                            b1.ToTable("Terminal");

                            b1.WithOwner()
                                .HasForeignKey("TerminalID");
                        });

                    b.OwnsOne("Merchants.Business.Entities.Terminal.TerminalSettings", "Settings", b1 =>
                        {
                            b1.Property<long>("TerminalID")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bigint")
                                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                            b1.Property<bool>("CvvRequired")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("bit")
                                .HasDefaultValue(false);

                            b1.Property<bool>("EnableDeletionOfUntransmittedTransactions")
                                .ValueGeneratedOnAdd()
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
                                .HasColumnType("bit")
                                .HasDefaultValue(false);

                            b1.Property<string>("PaymentButtonSettings")
                                .HasColumnType("nvarchar(max)")
                                .IsUnicode(true);

                            b1.Property<string>("RedirectPageSettings")
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
                    b.HasOne("Merchants.Business.Entities.Integration.ExternalSystem", "ExternalSystem")
                        .WithMany()
                        .HasForeignKey("ExternalSystemID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany("Integrations")
                        .HasForeignKey("TerminalID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Merchants.Business.Entities.User.UserTerminalMapping", b =>
                {
                    b.HasOne("Merchants.Business.Entities.Terminal.Terminal", "Terminal")
                        .WithMany()
                        .HasForeignKey("TerminalID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
