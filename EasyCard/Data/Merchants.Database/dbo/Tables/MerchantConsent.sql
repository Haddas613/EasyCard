CREATE TABLE [dbo].[MerchantConsent] (
    [MerchantConsentID] UNIQUEIDENTIFIER NOT NULL,
    [ConsentType]       TINYINT          NOT NULL,
    [Created]           DATETIME2 (7)    NOT NULL,
    [ConsentText]       NVARCHAR (MAX)   NULL,
    [ButtonText]        NVARCHAR (MAX)   NULL,
    [MerchantID]        UNIQUEIDENTIFIER NOT NULL,
    [TerminalID]        UNIQUEIDENTIFIER NOT NULL,
    [UserName]          NVARCHAR (MAX)   NULL,
    [FirstName]         NVARCHAR (MAX)   NULL,
    [LastName]          NVARCHAR (MAX)   NULL,
    CONSTRAINT [PK_MerchantConsent] PRIMARY KEY CLUSTERED ([MerchantConsentID] ASC)
);

