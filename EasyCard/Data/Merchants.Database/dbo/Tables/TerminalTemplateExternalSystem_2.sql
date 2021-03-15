CREATE TABLE [dbo].[TerminalTemplateExternalSystem] (
    [TerminalTemplateExternalSystemID] BIGINT         IDENTITY (1, 1) NOT NULL,
    [ExternalSystemID]                 BIGINT         NOT NULL,
    [Type]                             INT            NOT NULL,
    [TerminalTemplateID]               BIGINT         NOT NULL,
    [Settings]                         NVARCHAR (MAX) NULL,
    [UpdateTimestamp]                  ROWVERSION     NULL,
    [Created]                          DATETIME2 (7)  NULL,
    CONSTRAINT [PK_TerminalTemplateExternalSystem] PRIMARY KEY CLUSTERED ([TerminalTemplateExternalSystemID] ASC),
    CONSTRAINT [FK_TerminalTemplateExternalSystem_TerminalTemplate_TerminalTemplateID] FOREIGN KEY ([TerminalTemplateID]) REFERENCES [dbo].[TerminalTemplate] ([TerminalTemplateID])
);


GO
CREATE NONCLUSTERED INDEX [IX_TerminalTemplateExternalSystem_TerminalTemplateID]
    ON [dbo].[TerminalTemplateExternalSystem]([TerminalTemplateID] ASC);

