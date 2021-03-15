CREATE TABLE [dbo].[TerminalTemplate] (
    [TerminalTemplateID]     BIGINT         IDENTITY (1, 1) NOT NULL,
    [Label]                  NVARCHAR (50)  NOT NULL,
    [UpdateTimestamp]        ROWVERSION     NULL,
    [Created]                DATETIME2 (7)  NULL,
    [Settings]               NVARCHAR (MAX) NULL,
    [BillingSettings]        NVARCHAR (MAX) NULL,
    [InvoiceSettings]        NVARCHAR (MAX) NULL,
    [PaymentRequestSettings] NVARCHAR (MAX) NULL,
    [CheckoutSettings]       NVARCHAR (MAX) NULL,
    [Active]                 BIT            DEFAULT (CONVERT([bit],(0))) NOT NULL,
    [EnabledFeatures]        VARCHAR (MAX)  NULL,
    CONSTRAINT [PK_TerminalTemplate] PRIMARY KEY CLUSTERED ([TerminalTemplateID] ASC)
);

