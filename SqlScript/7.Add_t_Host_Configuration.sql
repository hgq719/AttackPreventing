IF NOT EXISTS
(
    SELECT name 
    FROM SYSOBJECTS
    WHERE TYPE ='U' AND NAME ='t_Host_Configuration'
)
BEGIN

CREATE TABLE [dbo].[t_Host_Configuration]
(
	[Id] INT IDENTITY(1,1) NOT NULL ,
	[Host] nvarchar(512) NOT NULL DEFAULT '', 
	[Threshold] int NOT NULL DEFAULT 0,
	[Period] int NOT NULL DEFAULT 0, 
	Constraint [PK_t_Host_Configuration] primary key clustered
	(
	  [Id] ASC
	)with( PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY] ;

INSERT INTO t_Host_Configuration VALUES('entchatserver.comm100.com',300,60);
INSERT INTO t_Host_Configuration VALUES('chatserver.comm100.com',300,60);
INSERT INTO t_Host_Configuration VALUES('chatserver7.comm100.com',300,60);
INSERT INTO t_Host_Configuration VALUES('www.comm100.com',200,60);
INSERT INTO t_Host_Configuration VALUES('ent.comm100.com',300,60);
INSERT INTO t_Host_Configuration VALUES('hosted.comm100.com',300,60);
END;