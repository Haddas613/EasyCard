﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Transactions.Business.Data;

namespace Transactions.Business.Migrations
{
    [DbContext(typeof(TransactionsContext))]
    [Migration("20200705144526_rejectionReason")]
    partial class rejectionReason
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.0-preview.6.20312.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Transactions.Business.Entities.CreditCardTokenDetails", b =>
                {
                    b.Property<Guid>("CreditCardTokenID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<bool>("Active")
                        .HasColumnType("bit");

                    b.Property<string>("CardExpiration")
                        .HasColumnType("varchar(5)")
                        .HasMaxLength(5)
                        .IsUnicode(false);

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasColumnType("varchar(20)")
                        .HasMaxLength(20)
                        .IsUnicode(false);

                    b.Property<string>("CardOwnerName")
                        .HasColumnType("nvarchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(true);

                    b.Property<string>("CardOwnerNationalID")
                        .HasColumnType("varchar(20)")
                        .HasMaxLength(20)
                        .IsUnicode(false);

                    b.Property<string>("CardVendor")
                        .HasColumnType("varchar(20)")
                        .HasMaxLength(20)
                        .IsUnicode(false);

                    b.Property<string>("CorrelationId")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<DateTime?>("Created")
                        .HasColumnType("datetime2");

                    b.Property<Guid?>("MerchantID")
                        .IsRequired()
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

                    b.Property<Guid?>("TerminalID")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("CreditCardTokenID");

                    b.ToTable("CreditCardTokenDetails");
                });

            modelBuilder.Entity("Transactions.Business.Entities.PaymentTransaction", b =>
                {
                    b.Property<Guid>("PaymentTransactionID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<long?>("AggregatorID")
                        .HasColumnType("bigint");

                    b.Property<short>("CardPresence")
                        .HasColumnType("smallint");

                    b.Property<string>("ConsumerIP")
                        .HasColumnName("ConsumerIP")
                        .HasColumnType("varchar(32)")
                        .HasMaxLength(32)
                        .IsUnicode(false);

                    b.Property<string>("CorrelationId")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<string>("CreditCardToken")
                        .HasColumnName("CreditCardToken")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.Property<short>("Currency")
                        .HasColumnType("smallint");

                    b.Property<int?>("CurrentDeal")
                        .HasColumnType("int");

                    b.Property<short?>("FinalizationStatus")
                        .HasColumnType("smallint");

                    b.Property<decimal>("InitialPaymentAmount")
                        .HasColumnType("decimal(19,4)");

                    b.Property<Guid?>("InitialTransactionID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("InstallmentPaymentAmount")
                        .HasColumnType("decimal(19,4)");

                    b.Property<long?>("InvoicingID")
                        .HasColumnType("bigint");

                    b.Property<short>("JDealType")
                        .HasColumnType("smallint");

                    b.Property<long?>("MarketerID")
                        .HasColumnType("bigint");

                    b.Property<Guid?>("MerchantID")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("MerchantIP")
                        .HasColumnName("MerchantIP")
                        .HasColumnType("varchar(32)")
                        .HasMaxLength(32)
                        .IsUnicode(false);

                    b.Property<int>("NumberOfPayments")
                        .HasColumnType("int");

                    b.Property<long?>("ProcessorID")
                        .HasColumnType("bigint");

                    b.Property<string>("RejectionMessage")
                        .HasColumnType("nvarchar(max)");

                    b.Property<short?>("RejectionReason")
                        .HasColumnType("smallint");

                    b.Property<short>("SpecialTransactionType")
                        .HasColumnType("smallint");

                    b.Property<short>("Status")
                        .HasColumnType("smallint");

                    b.Property<Guid?>("TerminalID")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal>("TotalAmount")
                        .HasColumnType("decimal(19,4)");

                    b.Property<decimal>("TransactionAmount")
                        .HasColumnType("decimal(19,4)");

                    b.Property<DateTime?>("TransactionDate")
                        .IsRequired()
                        .HasColumnType("date");

                    b.Property<DateTime?>("TransactionTimestamp")
                        .HasColumnType("datetime2");

                    b.Property<short>("TransactionType")
                        .HasColumnType("smallint");

                    b.Property<byte[]>("UpdateTimestamp")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTime?>("UpdatedDate")
                        .HasColumnType("datetime2");

                    b.HasKey("PaymentTransactionID");

                    b.ToTable("PaymentTransaction");
                });

            modelBuilder.Entity("Transactions.Business.Entities.TransactionHistory", b =>
                {
                    b.Property<Guid>("TransactionHistoryID")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CorrelationId")
                        .IsRequired()
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

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

                    b.Property<string>("OperationMessage")
                        .HasColumnType("nvarchar(250)")
                        .HasMaxLength(250)
                        .IsUnicode(true);

                    b.Property<Guid?>("PaymentTransactionID")
                        .IsRequired()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("SourceIP")
                        .HasColumnType("varchar(50)")
                        .HasMaxLength(50)
                        .IsUnicode(false);

                    b.HasKey("TransactionHistoryID");

                    b.HasIndex("PaymentTransactionID");

                    b.ToTable("TransactionHistory");
                });

            modelBuilder.Entity("Transactions.Business.Entities.PaymentTransaction", b =>
                {
                    b.OwnsOne("Transactions.Business.Entities.ClearingHouseTransactionDetails", "ClearingHouseTransactionDetails", b1 =>
                        {
                            b1.Property<Guid>("PaymentTransactionID")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<long?>("ClearingHouseTransactionID")
                                .HasColumnName("ClearingHouseTransactionID")
                                .HasColumnType("bigint");

                            b1.Property<Guid?>("MerchantReference")
                                .HasColumnName("ClearingHouseMerchantReference")
                                .HasColumnType("uniqueidentifier")
                                .HasMaxLength(50);

                            b1.HasKey("PaymentTransactionID");

                            b1.ToTable("PaymentTransaction");

                            b1.WithOwner()
                                .HasForeignKey("PaymentTransactionID");
                        });

                    b.OwnsOne("Transactions.Business.Entities.CreditCardDetails", "CreditCardDetails", b1 =>
                        {
                            b1.Property<Guid>("PaymentTransactionID")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("CardBin")
                                .HasColumnName("CardBin")
                                .HasColumnType("varchar(10)")
                                .HasMaxLength(10)
                                .IsUnicode(false);

                            b1.Property<string>("CardExpiration")
                                .HasColumnName("CardExpiration")
                                .HasColumnType("varchar(5)")
                                .HasMaxLength(5)
                                .IsUnicode(false);

                            b1.Property<string>("CardNumber")
                                .HasColumnName("CardNumber")
                                .HasColumnType("varchar(20)")
                                .HasMaxLength(20)
                                .IsUnicode(false);

                            b1.Property<string>("CardOwnerName")
                                .HasColumnName("CardOwnerName")
                                .HasColumnType("nvarchar(100)")
                                .HasMaxLength(100)
                                .IsUnicode(true);

                            b1.Property<string>("CardOwnerNationalID")
                                .HasColumnName("CardOwnerNationalID")
                                .HasColumnType("varchar(20)")
                                .HasMaxLength(20)
                                .IsUnicode(false);

                            b1.Property<string>("CardVendor")
                                .HasColumnName("CardVendor")
                                .HasColumnType("varchar(20)")
                                .HasMaxLength(20)
                                .IsUnicode(false);

                            b1.HasKey("PaymentTransactionID");

                            b1.ToTable("PaymentTransaction");

                            b1.WithOwner()
                                .HasForeignKey("PaymentTransactionID");
                        });

                    b.OwnsOne("Transactions.Business.Entities.DealDetails", "DealDetails", b1 =>
                        {
                            b1.Property<Guid>("PaymentTransactionID")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<string>("ConsumerEmail")
                                .HasColumnName("ConsumerEmail")
                                .HasColumnType("varchar(50)")
                                .HasMaxLength(50)
                                .IsUnicode(false);

                            b1.Property<string>("ConsumerPhone")
                                .HasColumnName("ConsumerPhone")
                                .HasColumnType("varchar(20)")
                                .HasMaxLength(20)
                                .IsUnicode(false);

                            b1.Property<string>("DealDescription")
                                .HasColumnName("DealDescription")
                                .HasColumnType("nvarchar(max)")
                                .IsUnicode(true);

                            b1.Property<string>("DealReference")
                                .HasColumnName("DealReference")
                                .HasColumnType("varchar(50)")
                                .HasMaxLength(50)
                                .IsUnicode(false);

                            b1.HasKey("PaymentTransactionID");

                            b1.ToTable("PaymentTransaction");

                            b1.WithOwner()
                                .HasForeignKey("PaymentTransactionID");
                        });

                    b.OwnsOne("Transactions.Business.Entities.ShvaTransactionDetails", "ShvaTransactionDetails", b1 =>
                        {
                            b1.Property<Guid>("PaymentTransactionID")
                                .HasColumnType("uniqueidentifier");

                            b1.Property<bool>("ManuallyTransmitted")
                                .HasColumnName("ManuallyTransmitted")
                                .HasColumnType("bit");

                            b1.Property<string>("ShvaDealID")
                                .HasColumnName("ShvaDealID")
                                .HasColumnType("varchar(30)")
                                .HasMaxLength(30)
                                .IsUnicode(false);

                            b1.Property<string>("ShvaShovarNumber")
                                .HasColumnName("ShvaShovarNumber")
                                .HasColumnType("varchar(20)")
                                .HasMaxLength(20)
                                .IsUnicode(false);

                            b1.Property<string>("ShvaTerminalID")
                                .HasColumnName("ShvaTerminalID")
                                .HasColumnType("varchar(20)")
                                .HasMaxLength(20)
                                .IsUnicode(false);

                            b1.Property<string>("ShvaTransmissionNumber")
                                .HasColumnName("ShvaTransmissionNumber")
                                .HasColumnType("varchar(20)")
                                .HasMaxLength(20)
                                .IsUnicode(false);

                            b1.Property<short?>("Solek")
                                .HasColumnName("Solek")
                                .HasColumnType("smallint");

                            b1.Property<DateTime?>("TransmissionDate")
                                .HasColumnName("ShvaTransmissionDate")
                                .HasColumnType("datetime2");

                            b1.HasKey("PaymentTransactionID");

                            b1.ToTable("PaymentTransaction");

                            b1.WithOwner()
                                .HasForeignKey("PaymentTransactionID");
                        });
                });

            modelBuilder.Entity("Transactions.Business.Entities.TransactionHistory", b =>
                {
                    b.HasOne("Transactions.Business.Entities.PaymentTransaction", "PaymentTransaction")
                        .WithMany()
                        .HasForeignKey("PaymentTransactionID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
