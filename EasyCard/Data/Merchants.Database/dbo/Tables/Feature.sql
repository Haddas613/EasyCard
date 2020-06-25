CREATE TABLE [dbo].[Feature] (
    [FeatureID]       BIGINT           IDENTITY (1, 1) NOT NULL,
    [NameEN]          NVARCHAR (50)    NULL,
    [NameHE]          NVARCHAR (50)    NULL,
    [Price]           DECIMAL (19, 4)  DEFAULT ((0.0)) NULL,
    [FeatureCode]     NVARCHAR (50)    NOT NULL,
    [UpdateTimestamp] ROWVERSION       NULL,
    [TerminalID]      UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Feature] PRIMARY KEY CLUSTERED ([FeatureID] ASC),
    CONSTRAINT [FK_Feature_Terminal_TerminalID] FOREIGN KEY ([TerminalID]) REFERENCES [dbo].[Terminal] ([TerminalID])
);




GO
CREATE NONCLUSTERED INDEX [IX_Feature_TerminalID]
    ON [dbo].[Feature]([TerminalID] ASC);

