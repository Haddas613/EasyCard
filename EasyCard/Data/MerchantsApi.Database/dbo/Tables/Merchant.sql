﻿CREATE TABLE [dbo].[Merchant] (
    [MerchantID]      BIGINT         IDENTITY (1, 1) NOT NULL,
    [UpdateTimestamp] ROWVERSION     NULL,
    [BusinessName]    NVARCHAR (50)  NOT NULL,
    [MarketingName]   NVARCHAR (50)  NULL,
    [BusinessID]      NVARCHAR (MAX) NULL,
    [ContactPerson]   NVARCHAR (50)  NULL,
    [Users]           VARCHAR (MAX)  NULL,
    [Created]         DATETIME2 (7)  NULL,
    CONSTRAINT [PK_Merchant] PRIMARY KEY CLUSTERED ([MerchantID] ASC)
);

