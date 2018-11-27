IF NOT EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='T_RateLimiting_Rules'
)
BEGIN

CREATE TABLE [dbo].[t_RateLimiting_Rules] (
    [Id]                      INT             IDENTITY (1, 1) NOT NULL,
    [ZoneId]                  NVARCHAR (512)  DEFAULT ('') NOT NULL,
    [OrderNo]                 INT             DEFAULT ((0)) NOT NULL,
    [Url]                     NVARCHAR (512)  DEFAULT ('') NOT NULL,
    [Threshold]               INT             DEFAULT ((0)) NOT NULL,
    [Period]                  INT             DEFAULT ((0)) NOT NULL,
    [Action]                  NVARCHAR (256)  DEFAULT ('Challenge') NOT NULL,
    [EnlargementFactor]       INT             DEFAULT ((0)) NOT NULL,
    [LatestTriggerTime]       DATETIME        NOT NULL,
    [RateLimitTriggerIpCount] INT             DEFAULT ((1)) NOT NULL,
    [RateLimitTriggerTime]    INT             DEFAULT ((1)) NOT NULL,
    [Remark]                  NVARCHAR (1024) DEFAULT ('') NOT NULL,
    [CreatedBy]               NVARCHAR (128)  DEFAULT ('') NOT NULL,
    [CreatedTime]             DATETIME        NOT NULL,
    CONSTRAINT [PK_t_RateLimiting_Rules] PRIMARY KEY CLUSTERED ([Id] ASC)
);



IF NOT EXISTS
(    SELECT *
	 FROM SYS.indexes
	 WHERE NAME = 'IX_T_RateLimiting_Rules'
)

CREATE NONCLUSTERED INDEX IX_T_RateLimiting_Rules_ZONEID ON T_RateLimiting_Rules
(
    [ZONEID] ASC
)
with( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
ON [PRIMARY];
END;