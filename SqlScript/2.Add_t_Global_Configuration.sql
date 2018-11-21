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
    CONSTRAINT [PK_t_Global_Configuration] PRIMARY KEY CLUSTERED ([Id] ASC)
);
END;