CREATE TABLE [dbo].[MasavFileRow] (
    [MasavFileRowID]       BIGINT           IDENTITY (1, 1) NOT NULL,
    [MasavFileID]          BIGINT           NULL,
    [PaymentTransactionID] UNIQUEIDENTIFIER NULL,
    [ConsumerID]           UNIQUEIDENTIFIER NULL,
    [Bankcode]             BIGINT           NULL,
    [BranchNumber]         BIGINT           NULL,
    [AccountNumber]        BIGINT           NULL,
    [NationalID]           BIGINT           NULL,
    [Amount]               DECIMAL (19, 4)  NULL,
    [IsPayed]              BIT              NULL,
    [SmsSent]              BIT              NOT NULL,
    [SmsSentDate]          DATETIME2 (7)    NULL,
    [ConsumerName]         NVARCHAR (50)    NULL,
    [InstituteNumber]      BIGINT           NULL,
    CONSTRAINT [PK_MasavFileRow] PRIMARY KEY CLUSTERED ([MasavFileRowID] ASC),
    CONSTRAINT [FK_MasavFileRow_MasavFile_MasavFileID] FOREIGN KEY ([MasavFileID]) REFERENCES [dbo].[MasavFile] ([MasavFileID])
);










GO
CREATE NONCLUSTERED INDEX [IX_MasavFileRow_MasavFileID]
    ON [dbo].[MasavFileRow]([MasavFileID] ASC);

