CREATE EXTERNAL TABLE [dbo].[Consumer] (
    [ConsumerID] UNIQUEIDENTIFIER NOT NULL,
    [MerchantID] UNIQUEIDENTIFIER NOT NULL,
    [Active] BIT NOT NULL,
    [UpdateTimestamp] ROWVERSION NULL,
    [ConsumerEmail] NVARCHAR (50) NULL,
    [ConsumerName] NVARCHAR (50) NOT NULL,
    [ConsumerPhone] NVARCHAR (50) NULL,
    [ConsumerAddress] NVARCHAR (MAX) NULL,
    [Created] DATETIME2 (7) NULL,
    [OperationDoneBy] NVARCHAR (50) NOT NULL,
    [OperationDoneByID] UNIQUEIDENTIFIER NULL,
    [CorrelationId] VARCHAR (50) NOT NULL,
    [SourceIP] VARCHAR (50) NULL,
    [TerminalID] UNIQUEIDENTIFIER NOT NULL,
    [ConsumerNationalID] NVARCHAR (50) NULL,
    [ExternalReference] NVARCHAR (50) NULL,
    [Origin] NVARCHAR (50) NULL,
    [ConsumerNote] NVARCHAR (512) NULL,
    [BillingDesktopRefNumber] NVARCHAR (50) NULL,
    [ConsumerSecondPhone] NVARCHAR (50) NULL,
    [BankDetails] NVARCHAR (MAX) NULL,
    [MergedFromConsumerID] UNIQUEIDENTIFIER NULL
)
    WITH (
    DATA_SOURCE = [MerchantsDb],
    SCHEMA_NAME = N'dbo',
    OBJECT_NAME = N'Consumer'
    );

