CREATE TABLE [dbo].[CreditCardTokenDetails] (
    [CreditCardTokenID]   UNIQUEIDENTIFIER NOT NULL,
    [CardNumber]          VARCHAR (20)     NOT NULL,
    [CardExpiration]      VARCHAR (5)      NULL,
    [CardVendor]          VARCHAR (20)     NULL,
    [CardOwnerName]       NVARCHAR (50)    NULL,
    [CardOwnerNationalID] VARCHAR (20)     NULL,
    [TerminalID]          UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]          UNIQUEIDENTIFIER NOT NULL,
    [Created]             DATETIME2 (7)    NULL,
    [Active]              BIT              NOT NULL,
    [OperationDoneBy]     NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]   UNIQUEIDENTIFIER NULL,
    [CorrelationId]       VARCHAR (50)     NULL,
    [SourceIP]            VARCHAR (50)     NULL,
    CONSTRAINT [PK_CreditCardTokenDetails] PRIMARY KEY CLUSTERED ([CreditCardTokenID] ASC)
);



