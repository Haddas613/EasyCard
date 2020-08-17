CREATE TABLE [dbo].[CurrencyRate] (
    [CurrencyRateID] BIGINT          IDENTITY (1, 1) NOT NULL,
    [Date]           DATE            NULL,
    [Currency]       SMALLINT        NOT NULL,
    [Rate]           DECIMAL (19, 4) NULL,
    CONSTRAINT [PK_CurrencyRate] PRIMARY KEY CLUSTERED ([CurrencyRateID] ASC)
);

