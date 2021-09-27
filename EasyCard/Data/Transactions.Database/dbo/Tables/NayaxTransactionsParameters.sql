CREATE TABLE [dbo].[NayaxTransactionsParameters] (
    [NayaxTransactionsParametersID]        UNIQUEIDENTIFIER NOT NULL,
    [NayaxTransactionsParametersTimestamp] DATETIME2 (7)    NULL,
    [PinPadTransactionID]                  VARCHAR (50)     NULL,
    [ShvaTranRecord]                       VARCHAR (600)    NULL,
    CONSTRAINT [PK_NayaxTransactionsParameters] PRIMARY KEY CLUSTERED ([NayaxTransactionsParametersID] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_NayaxTransactionsParameters_PinPadTransactionID]
    ON [dbo].[NayaxTransactionsParameters]([PinPadTransactionID] ASC) WHERE ([PinPadTransactionID] IS NOT NULL);

