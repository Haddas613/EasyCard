CREATE TABLE [dbo].[Feature] (
    [NameEN]          NVARCHAR (50)   NULL,
    [NameHE]          NVARCHAR (50)   NULL,
    [Price]           DECIMAL (19, 4) DEFAULT ((0.0)) NULL,
    [UpdateTimestamp] ROWVERSION      NULL,
    [FeatureID]       SMALLINT        DEFAULT (CONVERT([smallint],(0))) NOT NULL,
    CONSTRAINT [PK_Feature] PRIMARY KEY CLUSTERED ([FeatureID] ASC)
);






GO


