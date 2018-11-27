IF NOT EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='T_Users'
)
BEGIN

CREATE TABLE [dbo].[t_Users] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [UserName]    NVARCHAR (256) DEFAULT ('') NOT NULL,
    [CreatedBy]   INT            DEFAULT ((0)) NOT NULL,
    [CreatedTime] DATETIME       DEFAULT GETUTCDATE() NOT NULL,
    CONSTRAINT [PK_t_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
);

END;