CREATE TABLE [dbo].[SystemLog] (
    [ID]            UNIQUEIDENTIFIER NOT NULL,
    [LogLevel]      VARCHAR (20)     NULL,
    [CategoryName]  VARCHAR (250)    NULL,
    [Message]       NVARCHAR (MAX)   NULL,
    [UserName]      NVARCHAR (50)    NULL,
    [Timestamp]     DATETIME2 (7)    NULL,
    [CorrelationID] VARCHAR (250)    NULL,
    [Exception]     NVARCHAR (MAX)   NULL,
    [UserID]        VARCHAR (50)     NULL,
    [IP]            VARCHAR (50)     NULL,
    CONSTRAINT [PK_SystemLog] PRIMARY KEY CLUSTERED ([ID] ASC)
);



