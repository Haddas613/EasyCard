CREATE TABLE [dbo].[Consumer] (
    [ConsumerID]        UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]        UNIQUEIDENTIFIER NOT NULL,
    [Active]            BIT              NOT NULL,
    [UpdateTimestamp]   ROWVERSION       NULL,
    [ConsumerEmail]     NVARCHAR (50)    NOT NULL,
    [ConsumerName]      NVARCHAR (50)    NOT NULL,
    [ConsumerPhone]     NVARCHAR (50)    NOT NULL,
    [ConsumerAddress]   NVARCHAR (MAX)   NULL,
    [Created]           DATETIME2 (7)    NULL,
    [OperationDoneBy]   NVARCHAR (50)    NOT NULL,
    [OperationDoneByID] UNIQUEIDENTIFIER NULL,
    [CorrelationId]     VARCHAR (50)     NOT NULL,
    [SourceIP]          VARCHAR (50)     NULL,
    CONSTRAINT [PK_Consumer] PRIMARY KEY CLUSTERED ([ConsumerID] ASC),
    CONSTRAINT [FK_Consumer_Merchant_MerchantID] FOREIGN KEY ([MerchantID]) REFERENCES [dbo].[Merchant] ([MerchantID])
);


GO
CREATE NONCLUSTERED INDEX [IX_Consumer_MerchantID]
    ON [dbo].[Consumer]([MerchantID] ASC);

