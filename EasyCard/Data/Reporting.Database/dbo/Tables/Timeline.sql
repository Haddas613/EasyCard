CREATE TABLE [dbo].[Timeline] (
    [Date]  DATE NULL,
    [Year]  INT  NULL,
    [Month] INT  NULL,
    [Day]   INT  NULL
);


GO
CREATE NONCLUSTERED INDEX [IX_Year]
    ON [dbo].[Timeline]([Year] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Month]
    ON [dbo].[Timeline]([Month] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Day]
    ON [dbo].[Timeline]([Day] DESC);


GO
CREATE NONCLUSTERED INDEX [IX_Date]
    ON [dbo].[Timeline]([Date] DESC);

