CREATE TABLE [dbo].[SystemSettings] (
    [SystemSettingsID]       INT            NOT NULL,
    [Settings]               NVARCHAR (MAX) NULL,
    [BillingSettings]        NVARCHAR (MAX) NULL,
    [InvoiceSettings]        NVARCHAR (MAX) NULL,
    [PaymentRequestSettings] NVARCHAR (MAX) NULL,
    [CheckoutSettings]       NVARCHAR (MAX) NULL,
    [UpdateTimestamp]        ROWVERSION     NULL,
    CONSTRAINT [PK_SystemSettings] PRIMARY KEY CLUSTERED ([SystemSettingsID] ASC)
);

