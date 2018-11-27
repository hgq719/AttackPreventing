IF NOT EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='T_Global_Configuration'
)
BEGIN

CREATE TABLE [dbo].[t_Global_Configuration] (
    [Id]                   INT            IDENTITY (1, 1) NOT NULL,
    [EmailAddForWhiteList] NVARCHAR (512) DEFAULT ('') NOT NULL,
    [CancelBanIPTime]      INT            DEFAULT ((0)) NOT NULL,
    [ValidateCode]         NVARCHAR (256) DEFAULT ('') NOT NULL,
	[GlobalThreshold]      INT            DEFAULT ((0)) NOT NULL,
	[GlobalPeriod]      INT            DEFAULT ((0)) NOT NULL,
	[GlobalSample]      numeric(6,2)            DEFAULT ((0.0)) NOT NULL,
	[GlobalTimeSpan]    INT            DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_t_Global_Configuration] PRIMARY KEY CLUSTERED ([Id] ASC)
);

insert into  t_Global_Configuration values('elei.xu@comm100.com;summer.shen@comm100.com',120,'123456',200,60,1,60)
END;