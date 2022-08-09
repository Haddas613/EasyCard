CREATE TABLE [dbo].[NayaxTransactionsParameters] (
    [NayaxTransactionsParametersID]        UNIQUEIDENTIFIER NOT NULL,
    [NayaxTransactionsParametersTimestamp] DATETIME2 (7)    NULL,
    [PinPadTransactionID]                  VARCHAR (50)     NULL,
    [ShvaTranRecord]                       VARCHAR (MAX)    NULL,
    [PinPadTranRecordReceiptNumber]        VARCHAR (50)     NULL,
    CONSTRAINT [PK_NayaxTransactionsParameters] PRIMARY KEY CLUSTERED ([NayaxTransactionsParametersID] ASC)
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_NayaxTransactionsParameters_PinPadTransactionID]
    ON [dbo].[NayaxTransactionsParameters]([PinPadTransactionID] ASC) WHERE ([PinPadTransactionID] IS NOT NULL);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_NayaxTransactionsParameters_PinPadTranRecordReceiptNumber]
    ON [dbo].[NayaxTransactionsParameters]([PinPadTranRecordReceiptNumber] ASC) WHERE ([PinPadTranRecordReceiptNumber] IS NOT NULL);

