CREATE TABLE [dbo].[UserAudit] (
    [UserAuditID]          BIGINT         IDENTITY (1, 1) NOT NULL,
    [UserId]               NVARCHAR (450) NULL,
    [Email]                VARCHAR (50)   NOT NULL,
    [OperationCode]        VARCHAR (30)   NOT NULL,
    [OperationDescription] NVARCHAR (MAX) NULL,
    [SourceIP]             VARCHAR (50)   NULL,
    [OperationDate]        DATETIME2 (7)  NOT NULL,
    CONSTRAINT [PK_UserAudit] PRIMARY KEY CLUSTERED ([UserAuditID] ASC),
    CONSTRAINT [FK_UserAudit_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id])
);


GO
CREATE NONCLUSTERED INDEX [IX_UserAudit_UserId]
    ON [dbo].[UserAudit]([UserId] ASC);

