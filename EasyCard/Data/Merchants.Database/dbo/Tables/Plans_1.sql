CREATE TABLE [dbo].[Plans] (
    [PlanID]             BIGINT          IDENTITY (1, 1) NOT NULL,
    [Title]              NVARCHAR (MAX)  NULL,
    [Description]        NVARCHAR (MAX)  NULL,
    [Price]              DECIMAL (18, 2) NOT NULL,
    [Active]             BIT             NOT NULL,
    [TerminalTemplateID] BIGINT          NOT NULL,
    [UpdateTimestamp]    VARBINARY (MAX) NULL,
    CONSTRAINT [PK_Plans] PRIMARY KEY CLUSTERED ([PlanID] ASC),
    CONSTRAINT [FK_Plans_TerminalTemplate_TerminalTemplateID] FOREIGN KEY ([TerminalTemplateID]) REFERENCES [dbo].[TerminalTemplate] ([TerminalTemplateID])
);


GO
CREATE NONCLUSTERED INDEX [IX_Plans_TerminalTemplateID]
    ON [dbo].[Plans]([TerminalTemplateID] ASC);

