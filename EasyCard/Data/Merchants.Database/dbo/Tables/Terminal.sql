CREATE TABLE [dbo].[Terminal] (
    [TerminalID]                       UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]                       UNIQUEIDENTIFIER NOT NULL,
    [Label]                            NVARCHAR (50)    NOT NULL,
    [UpdateTimestamp]                  ROWVERSION       NULL,
    [Status]                           INT              NOT NULL,
    [ActivityStartDate]                DATETIME2 (7)    NULL,
    [Created]                          DATETIME2 (7)    NULL,
    [BillingSettings]                  NVARCHAR (MAX)   NULL,
    [CheckoutSettings]                 NVARCHAR (MAX)   NULL,
    [InvoiceSettings]                  NVARCHAR (MAX)   NULL,
    [PaymentRequestSettings]           NVARCHAR (MAX)   NULL,
    [Settings]                         NVARCHAR (MAX)   NULL,
    [SharedApiKey]                     VARBINARY (64)   NULL,
    [EnabledFeatures]                  VARCHAR (MAX)    NULL,
    [AggregatorTerminalReference]      VARCHAR (50)     NULL,
    [ProcessorTerminalReference]       VARCHAR (50)     NULL,
    [TerminalTemplateID]               BIGINT           NULL,
    [Updated]                          DATETIME2 (7)    NULL,
    [BankDetails]                      NVARCHAR (MAX)   NULL,
    [PinPadProcessorTerminalReference] NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_Terminal] PRIMARY KEY CLUSTERED ([TerminalID] ASC),
    CONSTRAINT [FK_Terminal_Merchant_MerchantID] FOREIGN KEY ([MerchantID]) REFERENCES [dbo].[Merchant] ([MerchantID])
);


















GO
CREATE NONCLUSTERED INDEX [IX_Terminal_MerchantID]
    ON [dbo].[Terminal]([MerchantID] ASC);

