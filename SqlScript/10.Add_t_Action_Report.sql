IF NOT EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='t_Action_Report'
)
BEGIN

CREATE TABLE [dbo].[t_Action_Report](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](512) DEFAULT ('') NOT NULL,
	[ZoneId] [nvarchar](256) DEFAULT ('') NOT NULL,
	[IP] [nvarchar](256) DEFAULT ('') NOT NULL,
	[HostName] [nvarchar](256) DEFAULT ('') NOT NULL,
	[Max] [int] DEFAULT ((0)) NOT NULL,
	[Min] [int] DEFAULT ((0)) NOT NULL,
	[Avg] [int] DEFAULT ((0)) NOT NULL,
	[FullUrl] [nvarchar](max) DEFAULT ('') NOT NULL,
	[CreatedTime] [datetime] DEFAULT(sysdatetime()) NOT NULL,
	[Mode] [nvarchar](256) DEFAULT ('') NOT NULL,
	[Count] [int] DEFAULT ((0)) NOT NULL,
	[MaxDisplay] [nvarchar](256) DEFAULT ('') NOT NULL,
	[MinDisplay] [nvarchar](256) DEFAULT ('') NOT NULL,
	[AvgDisplay] [nvarchar](256) DEFAULT ('') NOT NULL,
	[Remark] [nvarchar](512) DEFAULT ('') NOT NULL,
	CONSTRAINT [PK_t_Action_Report] PRIMARY KEY CLUSTERED ([Id] ASC)
);

END;