CREATE TABLE [dbo].[InvoiceHistory] (
    [InvoiceHistoryID]     UNIQUEIDENTIFIER NOT NULL,
    [InvoiceID]            UNIQUEIDENTIFIER NOT NULL,
    [OperationDate]        DATETIME2 (7)    NOT NULL,
    [OperationDoneBy]      NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]    UNIQUEIDENTIFIER NULL,
    [OperationCode]        SMALLINT         NOT NULL,
    [OperationDescription] NVARCHAR (MAX)   NULL,
    [OperationMessage]     NVARCHAR (250)   NULL,
    [CorrelationId]        VARCHAR (50)     NOT NULL,
    [SourceIP]             VARCHAR (50)     NULL,
    CONSTRAINT [PK_InvoiceHistory] PRIMARY KEY CLUSTERED ([InvoiceHistoryID] ASC),
    CONSTRAINT [FK_InvoiceHistory_Invoice_InvoiceID] FOREIGN KEY ([InvoiceID]) REFERENCES [dbo].[Invoice] ([InvoiceID]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_InvoiceHistory_InvoiceID]
    ON [dbo].[InvoiceHistory]([InvoiceID] ASC);

