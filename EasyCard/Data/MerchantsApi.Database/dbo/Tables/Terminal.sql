CREATE TABLE [dbo].[Terminal] (
    [TerminalID]                                         BIGINT         IDENTITY (1, 1) NOT NULL,
    [MerchantID]                                         BIGINT         NOT NULL,
    [Label]                                              NVARCHAR (50)  NOT NULL,
    [UpdateTimestamp]                                    ROWVERSION     NULL,
    [Status]                                             INT            NOT NULL,
    [ActivityStartDate]                                  DATETIME2 (7)  NULL,
    [Created]                                            DATETIME2 (7)  NULL,
    [Settings_RedirectPageSettings]                      NVARCHAR (MAX) NULL,
    [Settings_PaymentButtonSettings]                     NVARCHAR (MAX) NULL,
    [Settings_MinInstallments]                           INT            NULL,
    [Settings_MaxInstallments]                           INT            NULL,
    [Settings_MinCreditInstallments]                     INT            NULL,
    [Settings_MaxCreditInstallments]                     INT            NULL,
    [Settings_EnableDeletionOfUntransmittedTransactions] BIT            DEFAULT (CONVERT([bit],(0))) NULL,
    [Settings_NationalIDRequired]                        BIT            DEFAULT (CONVERT([bit],(0))) NULL,
    [Settings_CvvRequired]                               BIT            DEFAULT (CONVERT([bit],(0))) NULL,
    [BillingSettings_BillingNotificationsEmails]         NVARCHAR (MAX) NULL,
    [Users]                                              VARCHAR (MAX)  NULL,
    CONSTRAINT [PK_Terminal] PRIMARY KEY CLUSTERED ([TerminalID] ASC),
    CONSTRAINT [FK_Terminal_Merchant_MerchantID] FOREIGN KEY ([MerchantID]) REFERENCES [dbo].[Merchant] ([MerchantID]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Terminal_MerchantID]
    ON [dbo].[Terminal]([MerchantID] ASC);

