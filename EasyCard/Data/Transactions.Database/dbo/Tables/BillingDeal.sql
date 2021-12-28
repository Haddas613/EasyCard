CREATE TABLE [dbo].[BillingDeal] (
    [BillingDealID]               UNIQUEIDENTIFIER NOT NULL,
    [BillingDealTimestamp]        DATETIME2 (7)    NULL,
    [InitialTransactionID]        UNIQUEIDENTIFIER NULL,
    [TerminalID]                  UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]                  UNIQUEIDENTIFIER NOT NULL,
    [Currency]                    SMALLINT         NOT NULL,
    [TransactionAmount]           DECIMAL (19, 4)  NOT NULL,
    [TotalAmount]                 DECIMAL (19, 4)  NOT NULL,
    [CurrentDeal]                 INT              NULL,
    [CardNumber]                  VARCHAR (20)     NULL,
    [CardExpiration]              VARCHAR (5)      NULL,
    [CardVendor]                  VARCHAR (20)     NULL,
    [CardOwnerName]               NVARCHAR (100)   NULL,
    [CardOwnerNationalID]         VARCHAR (20)     NULL,
    [CardBin]                     VARCHAR (10)     NULL,
    [CreditCardToken]             UNIQUEIDENTIFIER NULL,
    [DealReference]               VARCHAR (50)     NULL,
    [DealDescription]             NVARCHAR (MAX)   NULL,
    [ConsumerEmail]               VARCHAR (50)     NULL,
    [ConsumerPhone]               VARCHAR (20)     NULL,
    [ConsumerID]                  UNIQUEIDENTIFIER NULL,
    [Items]                       NVARCHAR (MAX)   NULL,
    [BillingSchedule]             NVARCHAR (MAX)   NULL,
    [UpdatedDate]                 DATETIME2 (7)    NULL,
    [UpdateTimestamp]             ROWVERSION       NULL,
    [Active]                      BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [CorrelationId]               VARCHAR (50)     NULL,
    [OperationDoneBy]             NVARCHAR (50)    DEFAULT (N'') NOT NULL,
    [OperationDoneByID]           UNIQUEIDENTIFIER NULL,
    [SourceIP]                    VARCHAR (50)     NULL,
    [CurrentTransactionID]        UNIQUEIDENTIFIER NULL,
    [CurrentTransactionTimestamp] DATETIME2 (7)    NULL,
    [InvoiceSubject]              NVARCHAR (250)   NULL,
    [InvoiceType]                 SMALLINT         NULL,
    [NetTotal]                    DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [SendCCTo]                    NVARCHAR (MAX)   NULL,
    [VATRate]                     DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [VATTotal]                    DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [NextScheduledTransaction]    DATE             NULL,
    [IssueInvoice]                BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [DocumentOrigin]              SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [CustomerAddress]             NVARCHAR (MAX)   NULL,
    [PausedFrom]                  DATE             NULL,
    [PausedTo]                    DATE             NULL,
    [CardBrand]                   VARCHAR (20)     NULL,
    [ConsumerExternalReference]   VARCHAR (50)     NULL,
    [Solek]                       VARCHAR (20)     NULL,
    [Bank]                        INT              NULL,
    [BankAccount]                 NVARCHAR (50)    NULL,
    [BankBranch]                  INT              NULL,
    [PaymentType]                 SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [HasError]                    BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [LastError]                   NVARCHAR (MAX)   NULL,
    [LastErrorCorrelationID]      VARCHAR (50)     NULL,
    [ConsumerName]                NVARCHAR (50)    NULL,
    [ConsumerNationalID]          VARCHAR (20)     NULL,
    [CardExpirationDate]          DATE             NULL,
    [InvoiceOnly]                 BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [PaymentDetails]              NVARCHAR (MAX)   NULL,
    [Origin]                      NVARCHAR (50)    NULL,
    [InProgress]                  SMALLINT         NOT NULL,
    CONSTRAINT [PK_BillingDeal] PRIMARY KEY CLUSTERED ([BillingDealID] ASC)
);































