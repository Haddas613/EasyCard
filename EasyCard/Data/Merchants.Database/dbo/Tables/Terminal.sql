CREATE TABLE [dbo].[Terminal] (
    [TerminalID]                                UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]                                UNIQUEIDENTIFIER NOT NULL,
    [Label]                                     NVARCHAR (50)    NOT NULL,
    [UpdateTimestamp]                           ROWVERSION       NULL,
    [Status]                                    INT              NOT NULL,
    [ActivityStartDate]                         DATETIME2 (7)    NULL,
    [Created]                                   DATETIME2 (7)    NULL,
    [RedirectPageSettings]                      NVARCHAR (MAX)   NULL,
    [PaymentButtonSettings]                     NVARCHAR (MAX)   NULL,
    [Settings_MinInstallments]                  INT              NULL,
    [Settings_MaxInstallments]                  INT              NULL,
    [Settings_MinCreditInstallments]            INT              NULL,
    [Settings_MaxCreditInstallments]            INT              NULL,
    [EnableDeletionOfUntransmittedTransactions] BIT              DEFAULT (CONVERT([bit],(0))) NULL,
    [NationalIDRequired]                        BIT              DEFAULT (CONVERT([bit],(0))) NULL,
    [CvvRequired]                               BIT              DEFAULT (CONVERT([bit],(0))) NULL,
    [BillingNotificationsEmails]                NVARCHAR (MAX)   NULL,
    [J2Allowed]                                 BIT              DEFAULT (CONVERT([bit],(0))) NULL,
    [J5Allowed]                                 BIT              DEFAULT (CONVERT([bit],(0))) NULL,
    CONSTRAINT [PK_Terminal] PRIMARY KEY CLUSTERED ([TerminalID] ASC),
    CONSTRAINT [FK_Terminal_Merchant_MerchantID] FOREIGN KEY ([MerchantID]) REFERENCES [dbo].[Merchant] ([MerchantID])
);






GO
CREATE NONCLUSTERED INDEX [IX_Terminal_MerchantID]
    ON [dbo].[Terminal]([MerchantID] ASC);

