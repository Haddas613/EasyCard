CREATE EXTERNAL TABLE [dbo].[vPaymentTransactionItemDb] (
    [PaymentTransactionID] UNIQUEIDENTIFIER NULL,
    [ItemID] UNIQUEIDENTIFIER NULL,
    [ItemName] NVARCHAR (250) NULL,
    [Amount] DECIMAL (19, 4) NULL,
    [TransactionTimestamp] DATETIME2 (7) NULL,
    [Price] DECIMAL (19, 4) NULL,
    [Quantity] DECIMAL (19, 4) NULL,
    [TransactionDate] DATE NULL,
    [TerminalID] UNIQUEIDENTIFIER NULL,
    [MerchantID] UNIQUEIDENTIFIER NULL
)
    WITH (
    DATA_SOURCE = [TransactionsDb],
    SCHEMA_NAME = N'dbo',
    OBJECT_NAME = N'vPaymentTransactionItem'
    );





