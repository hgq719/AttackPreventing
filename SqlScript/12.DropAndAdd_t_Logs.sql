
IF NOT EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='T_Logs'
)
BEGIN

DROP TABLE dbo.t_Logs

CREATE TABLE [dbo].[t_Logs] (
    [Id]          INT             IDENTITY (1, 1) NOT NULL,
    [ZoneTableId]      INT  DEFAULT (0) NOT NULL,
    [LogLevel]    INT  DEFAULT (0) NOT NULL,
    [LogTime]     DATETIME        DEFAULT GETUTCDATE() NOT NULL,
    [LogOperator] NVARCHAR (256)  DEFAULT ('') NOT NULL,
    [IP]          NVARCHAR (256)  DEFAULT ('') NOT NULL,
    [Detail]      NVARCHAR (MAX)  DEFAULT ('') NOT NULL,
    [Remark]      NVARCHAR (1024) DEFAULT ('') NOT NULL
);


IF NOT EXISTS
(    SELECT *
	 FROM SYS.indexes
	 WHERE NAME = 'IX_T_Logs'
)

CREATE NONCLUSTERED INDEX [IX_T_Logs_LOGTIME] ON [dbo].[t_Logs]
(
	[LogTime] desc, [ZoneTableId] DESC, [LogLevel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


END;
