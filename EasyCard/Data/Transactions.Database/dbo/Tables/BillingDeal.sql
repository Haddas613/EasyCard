CREATE TABLE [dbo].[BillingDeal] (
    [BillingDealID]               UNIQUEIDENTIFIER NOT NULL,
    [BillingDealTimestamp]        DATETIME2 (7)    NULL,
    [InitialTransactionID]        UNIQUEIDENTIFIER NULL,
    [TerminalID]                  UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]                  UNIQUEIDENTIFIER NOT NULL,
    [Status]                      SMALLINT         NOT NULL,
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
    [CreditCardToken]             UNIQUEIDENTIFIER NOT NULL,
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
    [NextScheduledTransaction]    DATETIME2 (7)    NULL,
    CONSTRAINT [PK_BillingDeal] PRIMARY KEY CLUSTERED ([BillingDealID] ASC)
);







