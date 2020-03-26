CREATE TABLE [dbo].[CreditCardTokenDetails] (
    [CreditCardTokenID]   BIGINT         IDENTITY (1, 1) NOT NULL,
    [PublicKey]           VARCHAR (64)   NOT NULL,
    [Hash]                VARCHAR (256)  NOT NULL,
    [TerminalID]          BIGINT         NOT NULL,
    [MerchantID]          BIGINT         NOT NULL,
    [CardNumber]          VARCHAR (16)   NOT NULL,
    [CardExpiration]      VARCHAR (5)    NULL,
    [CardVendor]          NVARCHAR (MAX) NOT NULL,
    [CardOwnerNationalID] NVARCHAR (MAX) NOT NULL,
    [Created]             DATETIME2 (7)  NOT NULL,
    [Active]              BIT            DEFAULT (CONVERT([bit],(1))) NOT NULL,
    [CardBin]             NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_CreditCardTokenDetails] PRIMARY KEY CLUSTERED ([CreditCardTokenID] ASC)
);

