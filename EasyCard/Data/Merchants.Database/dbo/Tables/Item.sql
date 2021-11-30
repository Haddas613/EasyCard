CREATE TABLE [dbo].[Item] (
    [ItemID]                  UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]              UNIQUEIDENTIFIER NOT NULL,
    [Active]                  BIT              NOT NULL,
    [UpdateTimestamp]         ROWVERSION       NULL,
    [ItemName]                NVARCHAR (128)   NOT NULL,
    [Price]                   DECIMAL (19, 4)  NOT NULL,
    [Currency]                SMALLINT         NOT NULL,
    [Created]                 DATETIME2 (7)    NULL,
    [OperationDoneBy]         NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]       UNIQUEIDENTIFIER NULL,
    [CorrelationId]           VARCHAR (50)     NOT NULL,
    [SourceIP]                VARCHAR (50)     NULL,
    [ExternalReference]       NVARCHAR (50)    NULL,
    [BillingDesktopRefNumber] NVARCHAR (50)    NULL,
    [SKU]                     NVARCHAR (50)    NULL,
    [Origin]                  NVARCHAR (50)    NULL,
    CONSTRAINT [PK_Item] PRIMARY KEY CLUSTERED ([ItemID] ASC),
    CONSTRAINT [FK_Item_Merchant_MerchantID] FOREIGN KEY ([MerchantID]) REFERENCES [dbo].[Merchant] ([MerchantID])
);














GO
CREATE NONCLUSTERED INDEX [IX_Item_MerchantID]
    ON [dbo].[Item]([MerchantID] ASC);

