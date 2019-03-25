IF NOT EXISTS
(
    SELECT name 
	FROM syscolumns
	WHERE id=object_id('t_Zone_Info')   
	  AND name='LastAttactkTime'  
)
BEGIN
   ALTER TABLE t_Zone_Info ADD LastAttactkTime Datetime not null default DateAdd(DAY,-1,GETDATE());
END;
