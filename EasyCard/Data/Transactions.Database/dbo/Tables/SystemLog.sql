CREATE TABLE [dbo].[SystemLog] (
    [RowN]         BIGINT           IDENTITY (1, 1) NOT NULL,
    [ID]           UNIQUEIDENTIFIER NOT NULL,
    [LogLevel]     VARCHAR (20)     NULL,
    [CategoryName] VARCHAR (50)     NULL,
    [Message]      NVARCHAR (MAX)   NULL,
    [User]         NVARCHAR (50)    NULL,
    [Timestamp]    DATETIME2 (7)    NULL,
    CONSTRAINT [PK_SystemLog_1] PRIMARY KEY CLUSTERED ([RowN] ASC)
);

