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
    [ShvaShovarNumber]               VARCHAR (50)     NULL,
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
    [ShvaTranRecord]                 VARCHAR (700)    NULL,
    [CardBrand]                      VARCHAR (20)     NULL,
    [ShvaAuthNum]                    VARCHAR (20)     NULL,
    [UpayCreditCardCompanyCode]      NVARCHAR (64)    NULL,
    [UpayMerchantNumber]             NVARCHAR (64)    NULL,
    [UpayTransactionID]              NVARCHAR (64)    NULL,
    [UpayWebUrl]                     NVARCHAR (512)   NULL,
    [PinPadTransactionID]            VARCHAR (50)     NULL,
    [ProcessorResultCode]            INT              NULL,
    [CompRetailerNum]                VARCHAR (20)     NULL,
    [EmvSoftVersion]                 VARCHAR (20)     NULL,
    [PinPadDeviceID]                 VARCHAR (20)     NULL,
    [TelToGetAuthNum]                VARCHAR (20)     NULL,
    [ConsumerExternalReference]      VARCHAR (50)     NULL,
    [BankTransferBank]               INT              NULL,
    [BankTransferBankAccount]        NVARCHAR (50)    NULL,
    [BankTransferBankBranch]         INT              NULL,
    [BankTransferDueDate]            DATETIME2 (7)    NULL,
    [BankTransferReference]          NVARCHAR (50)    NULL,
    [MasavFileID]                    BIGINT           NULL,
    [ConsumerName]                   NVARCHAR (50)    NULL,
    [ConsumerNationalID]             VARCHAR (20)     NULL,
    [Extension]                      NVARCHAR (MAX)   NULL,
    [CardExpirationDate]             DATE             NULL,
    [PaymentIntentID]                UNIQUEIDENTIFIER NULL,
    [PinPadCorrelationID]            VARCHAR (50)     NULL,
    [PinPadUpdateReceiptNumber]      VARCHAR (50)     NULL,
    [ClearingHouseConcurrencyToken]  NVARCHAR (50)    NULL,
    [BitPaymentInitiationId]         VARCHAR (64)     NULL,
    [BitTransactionSerialId]         VARCHAR (64)     NULL,
    [BitMerchantNumber]              VARCHAR (20)     NULL,
    [BitRequestStatusCode]           VARCHAR (20)     NULL,
    [BitRequestStatusDescription]    NVARCHAR (50)    NULL,
    [TotalRefund]                    DECIMAL (19, 4)  NULL,
    [OperationDoneBy]                NVARCHAR (50)    NULL,
    [OperationDoneByID]              UNIQUEIDENTIFIER NULL,
    [ThreeDSServerTransID]           VARCHAR (50)     NULL,
    [ConsumerEcwidID]                VARCHAR (50)     NULL,
    [ConsumerWoocommerceID]          VARCHAR (50)     NULL,
    [Origin]                         NVARCHAR (50)    NULL,
    [Branch]                         NVARCHAR (50)    NULL,
    [Department]                     NVARCHAR (50)    NULL,
    [ExternalUserID]                 VARCHAR (50)     NULL,
    [ResponsiblePerson]              NVARCHAR (50)    NULL,
    [TransactionJ5ExpiredDate]       DATETIME2 (7)    NULL,
    CONSTRAINT [PK_PaymentTransaction] PRIMARY KEY CLUSTERED ([PaymentTransactionID] ASC)
);





































































GO
CREATE NONCLUSTERED INDEX [IX_PaymentTransaction_PinPadTransactionID]
    ON [dbo].[PaymentTransaction]([PinPadTransactionID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PaymentTransaction_TerminalID_PaymentTypeEnum_MasavFileID]
    ON [dbo].[PaymentTransaction]([TerminalID] ASC, [PaymentTypeEnum] ASC, [MasavFileID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PaymentTransaction_MerchantID_TerminalID]
    ON [dbo].[PaymentTransaction]([MerchantID] ASC, [TerminalID] ASC);

