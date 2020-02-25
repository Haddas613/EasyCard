CREATE TABLE [dbo].[UserTerminalMappings] (
    [UserTerminalMappingID] NVARCHAR (450) NOT NULL,
    [UserID]                NVARCHAR (MAX) NULL,
    [TerminalID]            BIGINT         NOT NULL,
    [OperationDate]         DATETIME2 (7)  NOT NULL,
    [OperationDoneBy]       NVARCHAR (MAX) NULL,
    [OperationDoneByID]     NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_UserTerminalMappings] PRIMARY KEY CLUSTERED ([UserTerminalMappingID] ASC),
    CONSTRAINT [FK_UserTerminalMappings_Terminal_TerminalID] FOREIGN KEY ([TerminalID]) REFERENCES [dbo].[Terminal] ([TerminalID]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_UserTerminalMappings_TerminalID]
    ON [dbo].[UserTerminalMappings]([TerminalID] ASC);

