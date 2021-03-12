CREATE TABLE [dbo].[_t_PaymentTransactions] (
    [ID]                   UNIQUEIDENTIFIER CONSTRAINT [DF__t_PaymentTransactions_ID] DEFAULT (newsequentialid()) NOT NULL,
    [PaymentTransactionID] UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT [PK__t_PaymentTransactions] PRIMARY KEY CLUSTERED ([ID] ASC)
);

