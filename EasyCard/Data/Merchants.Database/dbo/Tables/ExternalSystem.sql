CREATE TABLE [dbo].[ExternalSystem] (
    [ExternalSystemID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [Type]             INT            NOT NULL,
    [Name]             NVARCHAR (50)  NOT NULL,
    [Settings]         NVARCHAR (MAX) NOT NULL,
    [UpdateTimestamp]  ROWVERSION     NULL,
    CONSTRAINT [PK_ExternalSystem] PRIMARY KEY CLUSTERED ([ExternalSystemID] ASC)
);

