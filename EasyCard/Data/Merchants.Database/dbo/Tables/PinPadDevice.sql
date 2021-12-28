CREATE TABLE [dbo].[PinPadDevice] (
    [PinPadDeviceID]   UNIQUEIDENTIFIER NOT NULL,
    [DeviceTerminalID] NVARCHAR (64)    NULL,
    [PosName]          NVARCHAR (64)    NULL,
    [TerminalID]       UNIQUEIDENTIFIER NOT NULL,
    [Created]          DATETIME2 (7)    NOT NULL,
    [CorrelationId]    VARCHAR (50)     NOT NULL,
    CONSTRAINT [PK_PinPadDevice] PRIMARY KEY CLUSTERED ([PinPadDeviceID] ASC)
);

