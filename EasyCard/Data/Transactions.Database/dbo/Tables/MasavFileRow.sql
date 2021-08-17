CREATE TABLE [dbo].[MasavFileRow] (
    [MasavFileRowID]       BIGINT           IDENTITY (1, 1) NOT NULL,
    [MasavFileID]          BIGINT           NULL,
    [PaymentTransactionID] UNIQUEIDENTIFIER NULL,
    [ConsumerID]           UNIQUEIDENTIFIER NULL,
    [Bankcode]             INT              NULL,
    [BranchNumber]         INT              NULL,
    [AccountNumber]        INT              NULL,
    [NationalID]           INT              NULL,
    [Amount]               DECIMAL (19, 4)  NULL,
    [IsPayed]              BIT              NULL,
    [SmsSent]              BIT              NOT NULL,
    [SmsSentDate]          DATETIME2 (7)    NULL,
    [ConsumerName]         NVARCHAR (50)    NULL,
    CONSTRAINT [PK_MasavFileRow] PRIMARY KEY CLUSTERED ([MasavFileRowID] ASC),
    CONSTRAINT [FK_MasavFileRow_MasavFile_MasavFileID] FOREIGN KEY ([MasavFileID]) REFERENCES [dbo].[MasavFile] ([MasavFileID])
);






GO
CREATE NONCLUSTERED INDEX [IX_MasavFileRow_MasavFileID]
    ON [dbo].[MasavFileRow]([MasavFileID] ASC);

