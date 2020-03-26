CREATE TABLE [dbo].[TransactionHistory] (
    [TransactionHistoryID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [PaymentTransactionID] BIGINT         NOT NULL,
    [OperationDate]        DATETIME2 (7)  NOT NULL,
    [OperationDoneBy]      NVARCHAR (50)  NOT NULL,
    [OperationDoneByID]    VARCHAR (50)   NULL,
    [OperationCode]        VARCHAR (30)   NOT NULL,
    [OperationDescription] NVARCHAR (MAX) NULL,
    [OperationMessage]     NVARCHAR (250) NULL,
    [AdditionalDetails]    NVARCHAR (MAX) NULL,
    [CorrelationId]        VARCHAR (50)   NOT NULL,
    [IntegrationMessageId] VARCHAR (50)   NULL,
    CONSTRAINT [PK_TransactionHistory] PRIMARY KEY CLUSTERED ([TransactionHistoryID] ASC)
);

