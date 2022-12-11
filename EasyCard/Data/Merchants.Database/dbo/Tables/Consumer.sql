CREATE TABLE [dbo].[Consumer] (
    [ConsumerID]              UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]              UNIQUEIDENTIFIER NOT NULL,
    [Active]                  BIT              NOT NULL,
    [UpdateTimestamp]         ROWVERSION       NULL,
    [ConsumerEmail]           NVARCHAR (50)    NULL,
    [ConsumerName]            NVARCHAR (50)    NOT NULL,
    [ConsumerPhone]           NVARCHAR (50)    NULL,
    [ConsumerAddress]         NVARCHAR (MAX)   NULL,
    [Created]                 DATETIME2 (7)    NULL,
    [OperationDoneBy]         NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]       UNIQUEIDENTIFIER NULL,
    [CorrelationId]           VARCHAR (50)     NOT NULL,
    [SourceIP]                VARCHAR (50)     NULL,
    [TerminalID]              UNIQUEIDENTIFIER DEFAULT ('00000000-0000-0000-0000-000000000000') NOT NULL,
    [ConsumerNationalID]      NVARCHAR (50)    NULL,
    [ExternalReference]       NVARCHAR (50)    NULL,
    [Origin]                  NVARCHAR (50)    NULL,
    [ConsumerNote]            NVARCHAR (512)   NULL,
    [BillingDesktopRefNumber] NVARCHAR (50)    NULL,
    [ConsumerSecondPhone]     NVARCHAR (50)    NULL,
    [BankDetails]             NVARCHAR (MAX)   NULL,
    [MergedFromConsumerID]    UNIQUEIDENTIFIER NULL,
    [EcwidID]                 VARCHAR (50)     NULL,
    [WoocommerceID]           VARCHAR (50)     NULL,
    [HasCreditCard]           BIT              DEFAULT (CONVERT([bit],(0))) NOT NULL,
    CONSTRAINT [PK_Consumer] PRIMARY KEY CLUSTERED ([ConsumerID] ASC)
);




















GO



GO



GO


