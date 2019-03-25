IF NOT EXISTS
(
    SELECT name 
	FROM syscolumns
	WHERE id=object_id('T_Global_Configuration')   
	  AND name='CancelAttackTime'  
)
BEGIN
   ALTER TABLE T_Global_Configuration ADD CancelAttackTime INT not null default 5;
END;
