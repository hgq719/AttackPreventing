IF NOT EXISTS
(
	select 1 from syscolumns a,sysobjects b where a.id=object_id('t_Action_Report') 
and b.id=a.cdefault and a.name='IfCreateWhiteLimit' and b.name like 'DF%'
)
BEGIN
ALTER TABLE t_Action_Report ADD IfCreateWhiteLimit int NOT NULL DEFAULT(0);
END;
