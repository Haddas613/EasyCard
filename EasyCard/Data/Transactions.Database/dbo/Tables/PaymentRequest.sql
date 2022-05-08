CREATE TABLE [dbo].[PaymentRequest] (
    [PaymentRequestID]          UNIQUEIDENTIFIER NOT NULL,
    [PaymentRequestTimestamp]   DATETIME2 (7)    NULL,
    [TerminalID]                UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]                UNIQUEIDENTIFIER NOT NULL,
    [InvoiceType]               SMALLINT         NULL,
    [InvoiceSubject]            NVARCHAR (250)   NULL,
    [SendCCTo]                  NVARCHAR (MAX)   NULL,
    [Status]                    SMALLINT         NOT NULL,
    [Currency]                  SMALLINT         NOT NULL,
    [NumberOfPayments]          INT              NOT NULL,
    [PaymentRequestAmount]      DECIMAL (19, 4)  NOT NULL,
    [CardOwnerName]             NVARCHAR (100)   NULL,
    [CardOwnerNationalID]       VARCHAR (20)     NULL,
    [DealReference]             VARCHAR (50)     NULL,
    [DealDescription]           NVARCHAR (MAX)   NULL,
    [ConsumerEmail]             VARCHAR (50)     NULL,
    [ConsumerPhone]             VARCHAR (20)     NULL,
    [ConsumerID]                UNIQUEIDENTIFIER NULL,
    [Items]                     NVARCHAR (MAX)   NULL,
    [UpdatedDate]               DATETIME2 (7)    NULL,
    [UpdateTimestamp]           ROWVERSION       NULL,
    [OperationDoneBy]           NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]         UNIQUEIDENTIFIER NULL,
    [CorrelationId]             VARCHAR (50)     NULL,
    [SourceIP]                  VARCHAR (50)     NULL,
    [DueDate]                   DATETIME2 (7)    NULL,
    [PaymentTransactionID]      UNIQUEIDENTIFIER NULL,
    [FromAddress]               NVARCHAR (100)   NULL,
    [NetTotal]                  DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [RequestSubject]            NVARCHAR (250)   NULL,
    [VATRate]                   DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [VATTotal]                  DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [IssueInvoice]              BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [InitialPaymentAmount]      DECIMAL (19, 4)  NOT NULL,
    [InstallmentPaymentAmount]  DECIMAL (19, 4)  NOT NULL,
    [TotalAmount]               DECIMAL (19, 4)  NOT NULL,
    [DocumentOrigin]            SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [CustomerAddress]           NVARCHAR (MAX)   NULL,
    [PinPadTerminalID]          VARCHAR (16)     NULL,
    [AllowPinPad]               BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [IsRefund]                  BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [ConsumerExternalReference] VARCHAR (50)     NULL,
    [UserAmount]                BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [ConsumerName]              NVARCHAR (50)    NULL,
    [ConsumerNationalID]        VARCHAR (20)     NULL,
    [RedirectUrl]               NVARCHAR (MAX)   NULL,
    [Extension]                 NVARCHAR (MAX)   NULL,
    [ConsumerEcwidID]           VARCHAR (50)     NULL,
    [ConsumerWoocommerceID]     VARCHAR (50)     NULL,
    [PaymentRequestUrl]         NVARCHAR (250)   NULL,
    [Origin]                    NVARCHAR (50)    NULL,
    [Branch]                    NVARCHAR (50)    NULL,
    [Department]                NVARCHAR (50)    NULL,
    [ExternalUserID]            VARCHAR (50)     NULL,
    [ResponsiblePerson]         NVARCHAR (50)    NULL,
    [InvoiceDetails_Donation]   BIT              NULL,
    CONSTRAINT [PK_PaymentRequest] PRIMARY KEY CLUSTERED ([PaymentRequestID] ASC)
);





























