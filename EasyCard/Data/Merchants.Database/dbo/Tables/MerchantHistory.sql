CREATE TABLE [dbo].[MerchantHistory] (
    [MerchantHistoryID]    UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]           UNIQUEIDENTIFIER NOT NULL,
    [TerminalID]           UNIQUEIDENTIFIER NULL,
    [OperationDate]        DATETIME2 (7)    NOT NULL,
    [OperationCode]        SMALLINT         NOT NULL,
    [OperationDoneBy]      NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]    UNIQUEIDENTIFIER NULL,
    [OperationDescription] NVARCHAR (MAX)   NULL,
    [CorrelationId]        VARCHAR (50)     NULL,
    [SourceIP]             VARCHAR (50)     NULL,
    [ReasonForChange]      NVARCHAR (50)    NULL,
    CONSTRAINT [PK_MerchantHistory] PRIMARY KEY CLUSTERED ([MerchantHistoryID] ASC),
    CONSTRAINT [FK_MerchantHistory_Merchant_MerchantID] FOREIGN KEY ([MerchantID]) REFERENCES [dbo].[Merchant] ([MerchantID]),
    CONSTRAINT [FK_MerchantHistory_Terminal_TerminalID] FOREIGN KEY ([TerminalID]) REFERENCES [dbo].[Terminal] ([TerminalID])
);




GO
CREATE NONCLUSTERED INDEX [IX_MerchantHistory_TerminalID]
    ON [dbo].[MerchantHistory]([TerminalID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_MerchantHistory_MerchantID]
    ON [dbo].[MerchantHistory]([MerchantID] ASC);

