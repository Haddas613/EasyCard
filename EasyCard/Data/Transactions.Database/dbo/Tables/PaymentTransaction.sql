CREATE TABLE [dbo].[PaymentTransaction] (
    [PaymentTransactionID]           UNIQUEIDENTIFIER NOT NULL,
    [TransactionDate]                DATE             NOT NULL,
    [TransactionTimestamp]           DATETIME2 (7)    NULL,
    [InitialTransactionID]           UNIQUEIDENTIFIER NULL,
    [TerminalID]                     UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]                     UNIQUEIDENTIFIER NOT NULL,
    [ProcessorID]                    BIGINT           NULL,
    [AggregatorID]                   BIGINT           NULL,
    [InvoicingID]                    BIGINT           NULL,
    [MarketerID]                     BIGINT           NULL,
    [Status]                         SMALLINT         NOT NULL,
    [TransactionType]                SMALLINT         NOT NULL,
    [RejectionReason]                SMALLINT         NULL,
    [Currency]                       SMALLINT         NOT NULL,
    [CardPresence]                   SMALLINT         NOT NULL,
    [NumberOfPayments]               INT              NOT NULL,
    [TransactionAmount]              DECIMAL (19, 4)  NOT NULL,
    [InitialPaymentAmount]           DECIMAL (19, 4)  NOT NULL,
    [TotalAmount]                    DECIMAL (19, 4)  NOT NULL,
    [InstallmentPaymentAmount]       DECIMAL (19, 4)  NOT NULL,
    [CardNumber]                     VARCHAR (20)     NULL,
    [CardExpiration]                 VARCHAR (5)      NULL,
    [CardVendor]                     VARCHAR (20)     NULL,
    [CardOwnerName]                  NVARCHAR (100)   NULL,
    [CardOwnerNationalID]            VARCHAR (20)     NULL,
    [CardBin]                        VARCHAR (10)     NULL,
    [CreditCardToken]                UNIQUEIDENTIFIER NULL,
    [DealReference]                  VARCHAR (50)     NULL,
    [DealDescription]                NVARCHAR (MAX)   NULL,
    [ConsumerEmail]                  VARCHAR (50)     NULL,
    [ConsumerPhone]                  VARCHAR (20)     NULL,
    [ShvaShovarNumber]               VARCHAR (20)     NULL,
    [ShvaDealID]                     VARCHAR (30)     NULL,
    [ShvaTransmissionNumber]         VARCHAR (20)     NULL,
    [ShvaTerminalID]                 VARCHAR (20)     NULL,
    [ShvaTransmissionDate]           DATETIME2 (7)    NULL,
    [ManuallyTransmitted]            BIT              NULL,
    [ClearingHouseTransactionID]     BIGINT           NULL,
    [ClearingHouseMerchantReference] UNIQUEIDENTIFIER NULL,
    [UpdatedDate]                    DATETIME2 (7)    NULL,
    [UpdateTimestamp]                ROWVERSION       NULL,
    [ConsumerIP]                     VARCHAR (32)     NULL,
    [MerchantIP]                     VARCHAR (32)     NULL,
    [CorrelationId]                  VARCHAR (50)     NULL,
    [CurrentDeal]                    INT              NULL,
    [JDealType]                      SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [SpecialTransactionType]         SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [Solek]                          SMALLINT         NULL,
    [FinalizationStatus]             SMALLINT         NULL,
    [RejectionMessage]               NVARCHAR (MAX)   NULL,
    [ConsumerID]                     UNIQUEIDENTIFIER NULL,
    [Items]                          NVARCHAR (MAX)   NULL,
    [BillingDealID]                  UNIQUEIDENTIFIER NULL,
    [CustomerAddress]                NVARCHAR (MAX)   NULL,
    [NetTotal]                       DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [VATRate]                        DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [VATTotal]                       DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [InvoiceID]                      UNIQUEIDENTIFIER NULL,
    [IssueInvoice]                   BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [PaymentTypeEnum]                SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [PaymentRequestID]               UNIQUEIDENTIFIER NULL,
    [DocumentOrigin]                 SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [TerminalTemplateID]             BIGINT           NULL,
    [TotalDiscount]                  DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [ShvaTranRecord]                 VARCHAR (500)    NULL,
    CONSTRAINT [PK_PaymentTransaction] PRIMARY KEY CLUSTERED ([PaymentTransactionID] ASC)
);


































