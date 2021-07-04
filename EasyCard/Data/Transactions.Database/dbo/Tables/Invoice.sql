CREATE TABLE [dbo].[Invoice] (
    [InvoiceID]                UNIQUEIDENTIFIER NOT NULL,
    [InvoiceTimestamp]         DATETIME2 (7)    NULL,
    [InvoiceDate]              DATETIME2 (7)    NULL,
    [TerminalID]               UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]               UNIQUEIDENTIFIER NOT NULL,
    [InvoicingID]              BIGINT           NULL,
    [InvoiceType]              SMALLINT         NULL,
    [Status]                   SMALLINT         NOT NULL,
    [Currency]                 SMALLINT         NOT NULL,
    [NumberOfPayments]         INT              NOT NULL,
    [InvoiceAmount]            DECIMAL (19, 4)  NOT NULL,
    [CardOwnerName]            NVARCHAR (100)   NULL,
    [CardOwnerNationalID]      VARCHAR (20)     NULL,
    [DealReference]            VARCHAR (50)     NULL,
    [DealDescription]          NVARCHAR (MAX)   NULL,
    [ConsumerEmail]            VARCHAR (50)     NULL,
    [ConsumerPhone]            VARCHAR (20)     NULL,
    [ConsumerID]               UNIQUEIDENTIFIER NULL,
    [Items]                    NVARCHAR (MAX)   NULL,
    [UpdatedDate]              DATETIME2 (7)    NULL,
    [UpdateTimestamp]          ROWVERSION       NULL,
    [OperationDoneBy]          NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]        UNIQUEIDENTIFIER NULL,
    [CorrelationId]            VARCHAR (50)     NULL,
    [SourceIP]                 VARCHAR (50)     NULL,
    [InvoiceNumber]            NVARCHAR (20)    NULL,
    [InvoiceSubject]           NVARCHAR (250)   NULL,
    [SendCCTo]                 NVARCHAR (MAX)   NULL,
    [PaymentTransactionID]     UNIQUEIDENTIFIER NULL,
    [NetTotal]                 DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [VATRate]                  DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [VATTotal]                 DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [InitialPaymentAmount]     DECIMAL (19, 4)  NOT NULL,
    [InstallmentPaymentAmount] DECIMAL (19, 4)  NOT NULL,
    [TotalAmount]              DECIMAL (19, 4)  NOT NULL,
    [CopyDonwnloadUrl]         VARCHAR (MAX)    NULL,
    [DownloadUrl]              VARCHAR (MAX)    NULL,
    [CardExpiration]           VARCHAR (5)      NULL,
    [CardNumber]               VARCHAR (20)     NULL,
    [CardVendor]               VARCHAR (20)     NULL,
    [DocumentOrigin]           SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [TotalDiscount]            DECIMAL (19, 4)  DEFAULT ((0.0)) NOT NULL,
    [CustomerAddress]          NVARCHAR (MAX)   NULL,
    [CardBrand]                VARCHAR (20)     NULL,
    CONSTRAINT [PK_Invoice] PRIMARY KEY CLUSTERED ([InvoiceID] ASC)
);











