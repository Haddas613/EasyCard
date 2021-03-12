﻿CREATE EXTERNAL TABLE [dbo].[PaymentTransactionDb] (
    [PaymentTransactionID] UNIQUEIDENTIFIER NOT NULL,
    [TransactionDate] DATE NOT NULL,
    [TransactionTimestamp] DATETIME2 (7) NULL,
    [InitialTransactionID] UNIQUEIDENTIFIER NULL,
    [TerminalID] UNIQUEIDENTIFIER NOT NULL,
    [MerchantID] UNIQUEIDENTIFIER NOT NULL,
    [Status] SMALLINT NOT NULL,
    [TransactionType] SMALLINT NOT NULL,
    [Currency] SMALLINT NOT NULL,
    [CardPresence] SMALLINT NOT NULL,
    [NumberOfPayments] INT NOT NULL,
    [TransactionAmount] DECIMAL (19, 4) NOT NULL,
    [InitialPaymentAmount] DECIMAL (19, 4) NOT NULL,
    [TotalAmount] DECIMAL (19, 4) NOT NULL,
    [InstallmentPaymentAmount] DECIMAL (19, 4) NOT NULL,
    [CardNumber] VARCHAR (20) NULL,
    [CardVendor] VARCHAR (20) NULL,
    [CardOwnerName] NVARCHAR (100) NULL,
    [CardOwnerNationalID] VARCHAR (20) NULL,
    [CardBin] VARCHAR (10) NULL,
    [CreditCardToken] UNIQUEIDENTIFIER NULL,
    [ShvaTerminalID] VARCHAR (20) NULL,
    [UpdatedDate] DATETIME2 (7) NULL,
    [JDealType] SMALLINT NOT NULL,
    [SpecialTransactionType] SMALLINT NOT NULL,
    [Solek] SMALLINT NULL,
    [FinalizationStatus] SMALLINT NULL,
    [ConsumerID] UNIQUEIDENTIFIER NULL,
    [BillingDealID] UNIQUEIDENTIFIER NULL,
    [NetTotal] DECIMAL (19, 4) NOT NULL,
    [VATRate] DECIMAL (19, 4) NOT NULL,
    [VATTotal] DECIMAL (19, 4) NOT NULL,
    [InvoiceID] UNIQUEIDENTIFIER NULL,
    [IssueInvoice] BIT NOT NULL,
    [PaymentTypeEnum] SMALLINT NOT NULL,
    [PaymentRequestID] UNIQUEIDENTIFIER NULL,
    [DocumentOrigin] SMALLINT NOT NULL
)
    WITH (
    DATA_SOURCE = [TransactionsDb],
    SCHEMA_NAME = N'dbo',
    OBJECT_NAME = N'PaymentTransaction'
    );

