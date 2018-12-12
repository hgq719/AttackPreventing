IF NOT EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='t_Smtp_Queue'
)
BEGIN

CREATE TABLE [dbo].[t_Smtp_Queue] (
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](512) DEFAULT ('') NOT NULL,
	[Status] [int] DEFAULT ((0)) NOT NULL,
	[CreatedTime] [datetime] DEFAULT(sysdatetime()) NOT NULL,
	[SendedTime] [datetime] DEFAULT(sysdatetime()) NOT NULL,
	[Remark] [nvarchar](512) DEFAULT ('') NOT NULL,
    CONSTRAINT [PK_t_Smtp_Queue] PRIMARY KEY CLUSTERED ([Id] ASC)
);

END;