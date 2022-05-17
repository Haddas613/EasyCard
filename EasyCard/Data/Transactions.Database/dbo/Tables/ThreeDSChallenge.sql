CREATE TABLE [dbo].[ThreeDSChallenge] (
    [ThreeDSChallengeID]   UNIQUEIDENTIFIER NOT NULL,
    [MessageDate]          DATE             NULL,
    [MessageTimestamp]     DATETIME2 (7)    NULL,
    [Action]               VARCHAR (20)     NULL,
    [ThreeDSServerTransID] VARCHAR (50)     NULL,
    [TerminalID]           UNIQUEIDENTIFIER NULL,
    [MerchantID]           UNIQUEIDENTIFIER NULL,
    [TransStatus]          VARCHAR (20)     NULL,
    CONSTRAINT [PK_ThreeDSChallenge] PRIMARY KEY CLUSTERED ([ThreeDSChallengeID] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_ThreeDSChallenge_ThreeDSServerTransID]
    ON [dbo].[ThreeDSChallenge]([ThreeDSServerTransID] ASC);

