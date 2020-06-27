CREATE TABLE [dbo].[TransactionHistory] (
    [TransactionHistoryID] UNIQUEIDENTIFIER NOT NULL,
    [PaymentTransactionID] UNIQUEIDENTIFIER NOT NULL,
    [OperationDate]        DATETIME2 (7)    NOT NULL,
    [OperationDoneBy]      NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]    UNIQUEIDENTIFIER NULL,
    [OperationCode]        SMALLINT         NOT NULL,
    [OperationDescription] NVARCHAR (MAX)   NULL,
    [OperationMessage]     NVARCHAR (250)   NULL,
    [CorrelationId]        VARCHAR (50)     NOT NULL,
    [SourceIP]             VARCHAR (50)     NULL,
    CONSTRAINT [PK_TransactionHistory] PRIMARY KEY CLUSTERED ([TransactionHistoryID] ASC)
);



