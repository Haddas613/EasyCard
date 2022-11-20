CREATE TABLE [dbo].[CreditCardTokenDetails] (
    [CreditCardTokenID]                UNIQUEIDENTIFIER NOT NULL,
    [CardNumber]                       VARCHAR (20)     NOT NULL,
    [CardExpiration]                   VARCHAR (5)      NULL,
    [CardVendor]                       VARCHAR (20)     NULL,
    [CardOwnerName]                    NVARCHAR (50)    NULL,
    [CardOwnerNationalID]              VARCHAR (20)     NULL,
    [TerminalID]                       UNIQUEIDENTIFIER NOT NULL,
    [MerchantID]                       UNIQUEIDENTIFIER NOT NULL,
    [Created]                          DATETIME2 (7)    NULL,
    [Active]                           BIT              NOT NULL,
    [OperationDoneBy]                  NVARCHAR (50)    NOT NULL,
    [OperationDoneByID]                UNIQUEIDENTIFIER NULL,
    [CorrelationId]                    VARCHAR (50)     NULL,
    [SourceIP]                         VARCHAR (50)     NULL,
    [AuthNum]                          VARCHAR (20)     NULL,
    [AuthSolekNum]                     VARCHAR (20)     NULL,
    [ShvaDealID]                       VARCHAR (30)     NULL,
    [ShvaTransactionDate]              DATETIME2 (7)    NULL,
    [ConsumerEmail]                    VARCHAR (50)     NULL,
    [ConsumerID]                       UNIQUEIDENTIFIER NULL,
    [InitialTransactionID]             UNIQUEIDENTIFIER NULL,
    [DocumentOrigin]                   SMALLINT         DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    [CardBrand]                        NVARCHAR (20)    NULL,
    [Solek]                            NVARCHAR (20)    NULL,
    [CardExpirationDate]               DATE             NULL,
    [ReplacementOfTokenID]             UNIQUEIDENTIFIER NULL,
    [CardExpirationBeforeExtendedDate] DATE             NULL,
    [Extended]                         DATETIME2 (7)    NULL,
    CONSTRAINT [PK_CreditCardTokenDetails] PRIMARY KEY CLUSTERED ([CreditCardTokenID] ASC)
);




















GO
CREATE NONCLUSTERED INDEX [IX_CreditCardTokenDetails_Active_TerminalID_ConsumerID]
    ON [dbo].[CreditCardTokenDetails]([Active] ASC, [TerminalID] ASC, [ConsumerID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_CreditCardTokenDetails_Active_MerchantID_ConsumerID]
    ON [dbo].[CreditCardTokenDetails]([Active] ASC, [MerchantID] ASC, [ConsumerID] ASC);

