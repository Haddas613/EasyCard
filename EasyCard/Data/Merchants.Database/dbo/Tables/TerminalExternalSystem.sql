CREATE TABLE [dbo].[TerminalExternalSystem] (
    [TerminalExternalSystemID]   BIGINT           IDENTITY (1, 1) NOT NULL,
    [ExternalSystemID]           BIGINT           NOT NULL,
    [TerminalID]                 UNIQUEIDENTIFIER NOT NULL,
    [ExternalProcessorReference] VARCHAR (50)     NULL,
    [Settings]                   NVARCHAR (MAX)   NULL,
    [UpdateTimestamp]            ROWVERSION       NULL,
    [Created]                    DATETIME2 (7)    NULL,
    [Type]                       INT              DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TerminalExternalSystem] PRIMARY KEY CLUSTERED ([TerminalExternalSystemID] ASC),
    CONSTRAINT [FK_TerminalExternalSystem_Terminal_TerminalID] FOREIGN KEY ([TerminalID]) REFERENCES [dbo].[Terminal] ([TerminalID]) ON DELETE CASCADE
);




GO
CREATE NONCLUSTERED INDEX [IX_TerminalExternalSystem_TerminalID]
    ON [dbo].[TerminalExternalSystem]([TerminalID] ASC);


GO


