CREATE TABLE [dbo].[PaymentTransaction] (
    [PaymentTransactionID]     UNIQUEIDENTIFIER NULL,
    [TransactionDate]          DATE             NULL,
    [TransactionTimestamp]     DATETIME2 (7)    NULL,
    [InitialTransactionID]     UNIQUEIDENTIFIER NULL,
    [TerminalID]               UNIQUEIDENTIFIER NULL,
    [MerchantID]               UNIQUEIDENTIFIER NULL,
    [Status]                   SMALLINT         NULL,
    [TransactionType]          SMALLINT         NULL,
    [Currency]                 SMALLINT         NULL,
    [CardPresence]             SMALLINT         NULL,
    [NumberOfPayments]         INT              NULL,
    [TransactionAmount]        DECIMAL (19, 4)  NULL,
    [InitialPaymentAmount]     DECIMAL (19, 4)  NULL,
    [TotalAmount]              DECIMAL (19, 4)  NULL,
    [InstallmentPaymentAmount] DECIMAL (19, 4)  NULL,
    [CardNumber]               VARCHAR (20)     NULL,
    [CardVendor]               VARCHAR (20)     NULL,
    [CardOwnerName]            NVARCHAR (100)   NULL,
    [CardOwnerNationalID]      VARCHAR (20)     NULL,
    [CardBin]                  VARCHAR (10)     NULL,
    [CreditCardToken]          UNIQUEIDENTIFIER NULL,
    [ShvaTerminalID]           VARCHAR (20)     NULL,
    [UpdatedDate]              DATETIME2 (7)    NULL,
    [JDealType]                SMALLINT         NULL,
    [SpecialTransactionType]   SMALLINT         NULL,
    [Solek]                    SMALLINT         NULL,
    [FinalizationStatus]       SMALLINT         NULL,
    [ConsumerID]               UNIQUEIDENTIFIER NULL,
    [BillingDealID]            UNIQUEIDENTIFIER NULL,
    [NetTotal]                 DECIMAL (19, 4)  NULL,
    [VATRate]                  DECIMAL (19, 4)  NULL,
    [VATTotal]                 DECIMAL (19, 4)  NULL,
    [InvoiceID]                UNIQUEIDENTIFIER NULL,
    [IssueInvoice]             BIT              NULL,
    [PaymentTypeEnum]          SMALLINT         NULL,
    [PaymentRequestID]         UNIQUEIDENTIFIER NULL,
    [DocumentOrigin]           SMALLINT         NULL,
    [ConsumerRef]              NVARCHAR (100)   NULL
);




GO
CREATE NONCLUSTERED INDEX [IX_VATTotal]
    ON [dbo].[PaymentTransaction]([VATTotal] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_VATRate]
    ON [dbo].[PaymentTransaction]([VATRate] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UpdatedDate]
    ON [dbo].[PaymentTransaction]([UpdatedDate] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_TransactionType]
    ON [dbo].[PaymentTransaction]([TransactionType] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TransactionTimestamp]
    ON [dbo].[PaymentTransaction]([TransactionTimestamp] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_TransactionDate]
    ON [dbo].[PaymentTransaction]([TransactionDate] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TransactionAmount]
    ON [dbo].[PaymentTransaction]([TransactionAmount] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TotalAmount]
    ON [dbo].[PaymentTransaction]([TotalAmount] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TerminalID]
    ON [dbo].[PaymentTransaction]([TerminalID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Status]
    ON [dbo].[PaymentTransaction]([Status] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SpecialTransactionType]
    ON [dbo].[PaymentTransaction]([SpecialTransactionType] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Solek]
    ON [dbo].[PaymentTransaction]([Solek] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ShvaTerminalID]
    ON [dbo].[PaymentTransaction]([ShvaTerminalID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PaymentTypeEnum]
    ON [dbo].[PaymentTransaction]([PaymentTypeEnum] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PaymentRequestID]
    ON [dbo].[PaymentTransaction]([PaymentRequestID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_NumberOfPayments]
    ON [dbo].[PaymentTransaction]([NumberOfPayments] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_NetTotal]
    ON [dbo].[PaymentTransaction]([NetTotal] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_MerchantID]
    ON [dbo].[PaymentTransaction]([MerchantID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_JDealType]
    ON [dbo].[PaymentTransaction]([JDealType] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_IssueInvoice]
    ON [dbo].[PaymentTransaction]([IssueInvoice] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceID]
    ON [dbo].[PaymentTransaction]([InvoiceID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_InstallmentPaymentAmount]
    ON [dbo].[PaymentTransaction]([InstallmentPaymentAmount] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_InitialTransactionID]
    ON [dbo].[PaymentTransaction]([InitialTransactionID] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_InitialPaymentAmount]
    ON [dbo].[PaymentTransaction]([InitialPaymentAmount] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_FinalizationStatus]
    ON [dbo].[PaymentTransaction]([FinalizationStatus] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_DocumentOrigin]
    ON [dbo].[PaymentTransaction]([DocumentOrigin] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Currency]
    ON [dbo].[PaymentTransaction]([Currency] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CreditCardToken]
    ON [dbo].[PaymentTransaction]([CreditCardToken] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_ConsumerID]
    ON [dbo].[PaymentTransaction]([ConsumerID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CardVendor]
    ON [dbo].[PaymentTransaction]([CardVendor] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CardPresence]
    ON [dbo].[PaymentTransaction]([CardPresence] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CardOwnerNationalID]
    ON [dbo].[PaymentTransaction]([CardOwnerNationalID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CardOwnerName]
    ON [dbo].[PaymentTransaction]([CardOwnerName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CardNumber]
    ON [dbo].[PaymentTransaction]([CardNumber] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CardBin]
    ON [dbo].[PaymentTransaction]([CardBin] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_BillingDealID]
    ON [dbo].[PaymentTransaction]([BillingDealID] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_PaymentTransactionID]
    ON [dbo].[PaymentTransaction]([PaymentTransactionID] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_ConsumerRef]
    ON [dbo].[PaymentTransaction]([ConsumerRef] ASC);

