CREATE TABLE [dbo].[UserPasswordSnapshot] (
    [UserPasswordSnapshotID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [UserId]                 NVARCHAR (MAX) NOT NULL,
    [HashedPassword]         VARCHAR (512)  NOT NULL,
    [SecurityStamp]          VARCHAR (512)  NOT NULL,
    [Created]                DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_UserPasswordSnapshot] PRIMARY KEY CLUSTERED ([UserPasswordSnapshotID] ASC)
);

