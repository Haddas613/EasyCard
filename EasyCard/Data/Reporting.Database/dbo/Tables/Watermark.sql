CREATE TABLE [dbo].[Watermark] (
    [TableName]   VARCHAR (20)  NOT NULL,
    [LastUpdated] DATETIME2 (7) NULL,
    CONSTRAINT [PK_Watermark] PRIMARY KEY CLUSTERED ([TableName] ASC)
);

