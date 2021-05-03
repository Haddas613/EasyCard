CREATE TABLE [dbo].[ShvaTerminal] (
    [UserName]       NVARCHAR (64) NULL,
    [Password]       NVARCHAR (64) NULL,
    [MerchantNumber] NVARCHAR (64) DEFAULT (N'') NOT NULL,
    CONSTRAINT [PK_ShvaTerminal] PRIMARY KEY CLUSTERED ([MerchantNumber] ASC)
);

