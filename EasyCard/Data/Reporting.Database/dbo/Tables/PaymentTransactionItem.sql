CREATE TABLE [dbo].[PaymentTransactionItem] (
    [PaymentTransactionID] UNIQUEIDENTIFIER NULL,
    [ItemID]               UNIQUEIDENTIFIER NULL,
    [ItemName]             NVARCHAR (250)   NULL,
    [Amount]               DECIMAL (19, 4)  NULL,
    [TransactionTimestamp] DATETIME2 (7)    NULL,
    [Price]                DECIMAL (19, 4)  NULL,
    [Quantity]             DECIMAL (19, 4)  NULL,
    [TransactionDate]      DATE             NULL,
    [TerminalID]           UNIQUEIDENTIFIER NULL,
    [MerchantID]           UNIQUEIDENTIFIER NULL
);






GO
CREATE NONCLUSTERED INDEX [IX_TransactionTimestamp]
    ON [dbo].[PaymentTransactionItem]([TransactionTimestamp] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Quantity]
    ON [dbo].[PaymentTransactionItem]([Quantity] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Price]
    ON [dbo].[PaymentTransactionItem]([Price] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_PaymentTransactionID]
    ON [dbo].[PaymentTransactionItem]([PaymentTransactionID] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_ItemName]
    ON [dbo].[PaymentTransactionItem]([ItemName] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_ItemID]
    ON [dbo].[PaymentTransactionItem]([ItemID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Amount]
    ON [dbo].[PaymentTransactionItem]([Amount] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_TransactionDate]
    ON [dbo].[PaymentTransactionItem]([TransactionDate] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_TerminalID]
    ON [dbo].[PaymentTransactionItem]([TerminalID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_MerchantID]
    ON [dbo].[PaymentTransactionItem]([MerchantID] ASC);

