CREATE TABLE [dbo].[UserTerminalMapping] (
    [UserTerminalMappingID] BIGINT           IDENTITY (1, 1) NOT NULL,
    [UserID]                UNIQUEIDENTIFIER NOT NULL,
    [TerminalID]            UNIQUEIDENTIFIER NOT NULL,
    [OperationDate]         DATETIME2 (7)    NOT NULL,
    [OperationDoneBy]       NVARCHAR (50)    NULL,
    [OperationDoneByID]     UNIQUEIDENTIFIER NULL,
    [DisplayName]           NVARCHAR (50)    NULL,
    [Email]                 NVARCHAR (50)    NULL,
    [Roles]                 VARCHAR (MAX)    NULL,
    CONSTRAINT [PK_UserTerminalMapping] PRIMARY KEY CLUSTERED ([UserTerminalMappingID] ASC),
    CONSTRAINT [FK_UserTerminalMapping_Terminal_TerminalID] FOREIGN KEY ([TerminalID]) REFERENCES [dbo].[Terminal] ([TerminalID]) ON DELETE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserTerminalMapping_UserID_TerminalID]
    ON [dbo].[UserTerminalMapping]([UserID] ASC, [TerminalID] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_UserTerminalMapping_TerminalID]
    ON [dbo].[UserTerminalMapping]([TerminalID] ASC);

