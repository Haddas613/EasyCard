CREATE TABLE [dbo].[MasavFile] (
    [MasavFileID]       BIGINT          IDENTITY (1, 1) NOT NULL,
    [MasavFileDate]     DATETIME2 (7)   NULL,
    [PayedDate]         DATETIME2 (7)   NULL,
    [TransactionAmount] DECIMAL (19, 4) NULL,
    [StorageReference]  NVARCHAR (MAX)  NULL,
    [InstituteNumber]   INT             NULL,
    [SendingInstitute]  INT             NULL,
    [InstituteName]     NVARCHAR (250)  NULL,
    [Currency]          SMALLINT        NOT NULL,
    CONSTRAINT [PK_MasavFile] PRIMARY KEY CLUSTERED ([MasavFileID] ASC)
);

