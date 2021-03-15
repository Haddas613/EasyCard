CREATE TABLE [dbo].[Impersonation] (
    [UserId]     UNIQUEIDENTIFIER NOT NULL,
    [MerchantID] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK_Impersonation] PRIMARY KEY CLUSTERED ([UserId] ASC)
);

