CREATE TABLE [dbo].[SystemLog] (
    [ID]            UNIQUEIDENTIFIER NOT NULL,
    [LogLevel]      VARCHAR (20)     NULL,
    [CategoryName]  VARCHAR (250)    NULL,
    [Message]       NVARCHAR (MAX)   NULL,
    [UserName]      NVARCHAR (50)    NULL,
    [Timestamp]     DATETIME2 (7)    NULL,
    [CorrelationID] VARCHAR (250)    NULL,
    [Exception]     NVARCHAR (MAX)   NULL,
    [UserID]        UNIQUEIDENTIFIER NULL,
    [IP]            VARCHAR (50)     NULL,
    [ApiName]       VARCHAR (50)     NULL,
    [Host]          NVARCHAR (MAX)   NULL,
    [Url]           NVARCHAR (MAX)   NULL,
    [MachineName]   VARCHAR (50)     NULL,
    CONSTRAINT [PK_SystemLog] PRIMARY KEY CLUSTERED ([ID] DESC)
);






GO
CREATE NONCLUSTERED INDEX [IX_SystemLog_Timestamp]
    ON [dbo].[SystemLog]([Timestamp] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_SystemLog_LogLevel]
    ON [dbo].[SystemLog]([LogLevel] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_SystemLog_CorrelationID]
    ON [dbo].[SystemLog]([CorrelationID] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_SystemLog_ApiName]
    ON [dbo].[SystemLog]([ApiName] ASC);

