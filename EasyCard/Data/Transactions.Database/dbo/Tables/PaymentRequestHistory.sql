CREATE TABLE [dbo].[PaymentRequestHistory] (
    [PaymentRequestHistoryID] UNIQUEIDENTIFIER NOT NULL,
    [PaymentRequestID]        UNIQUEIDENTIFIER NOT NULL,
    [OperationDate]           DATETIME2 (7)    NOT NULL,
    [OperationDoneBy]         NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]       UNIQUEIDENTIFIER NULL,
    [OperationCode]           SMALLINT         NOT NULL,
    [OperationDescription]    NVARCHAR (MAX)   NULL,
    [OperationMessage]        NVARCHAR (250)   NULL,
    [CorrelationId]           VARCHAR (50)     NOT NULL,
    [SourceIP]                VARCHAR (50)     NULL,
    CONSTRAINT [PK_PaymentRequestHistory] PRIMARY KEY CLUSTERED ([PaymentRequestHistoryID] ASC),
    CONSTRAINT [FK_PaymentRequestHistory_PaymentRequest_PaymentRequestID] FOREIGN KEY ([PaymentRequestID]) REFERENCES [dbo].[PaymentRequest] ([PaymentRequestID]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_PaymentRequestHistory_PaymentRequestID]
    ON [dbo].[PaymentRequestHistory]([PaymentRequestID] ASC);

