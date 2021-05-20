CREATE TABLE [dbo].[BillingDealHistory] (
    [BillingDealHistoryID] UNIQUEIDENTIFIER NOT NULL,
    [BillingDealID]        UNIQUEIDENTIFIER NOT NULL,
    [OperationDate]        DATETIME2 (7)    NOT NULL,
    [OperationDoneBy]      NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]    UNIQUEIDENTIFIER NULL,
    [OperationCode]        SMALLINT         NOT NULL,
    [OperationDescription] NVARCHAR (MAX)   NULL,
    [OperationMessage]     NVARCHAR (250)   NULL,
    [CorrelationId]        VARCHAR (50)     NOT NULL,
    [SourceIP]             VARCHAR (50)     NULL,
    CONSTRAINT [PK_BillingDealHistory] PRIMARY KEY CLUSTERED ([BillingDealHistoryID] ASC),
    CONSTRAINT [FK_BillingDealHistory_BillingDeal_BillingDealID] FOREIGN KEY ([BillingDealID]) REFERENCES [dbo].[BillingDeal] ([BillingDealID]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_BillingDealHistory_BillingDealID]
    ON [dbo].[BillingDealHistory]([BillingDealID] ASC);

