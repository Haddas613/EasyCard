CREATE TABLE [dbo].[MasavFile] (
    [MasavFileID]        BIGINT           IDENTITY (1, 1) NOT NULL,
    [MasavFileDate]      DATE             NULL,
    [PayedDate]          DATETIME2 (7)    NULL,
    [TotalAmount]        DECIMAL (19, 4)  NULL,
    [StorageReference]   NVARCHAR (MAX)   NULL,
    [InstituteNumber]    INT              NULL,
    [SendingInstitute]   INT              NULL,
    [InstituteName]      NVARCHAR (250)   NULL,
    [Currency]           SMALLINT         NOT NULL,
    [TerminalID]         UNIQUEIDENTIFIER NULL,
    [MasavFileTimestamp] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_MasavFile] PRIMARY KEY CLUSTERED ([MasavFileID] ASC)
);






GO
CREATE NONCLUSTERED INDEX [IX_MasavFile_TerminalID]
    ON [dbo].[MasavFile]([TerminalID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_MasavFile_MasavFileDate]
    ON [dbo].[MasavFile]([MasavFileDate] ASC);

