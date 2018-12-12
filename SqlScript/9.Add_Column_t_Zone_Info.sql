IF NOT EXISTS
(
    SELECT name 
	FROM syscolumns
	WHERE id=object_id('t_Zone_Info')   
	  AND name='IfAnalyzeByHostRule'  
)
BEGIN
   ALTER TABLE t_Zone_Info ADD IfAnalyzeByHostRule int not null default 0;
END;