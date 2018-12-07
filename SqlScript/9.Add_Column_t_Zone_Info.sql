IF NOT EXISTS
(
    SELECT name 
	FROM syscolumns
	WHERE id=object_id('t_Zone_Info')   
	  AND name='ThresholdForHost'  
)
BEGIN
   ALTER TABLE t_Zone_Info ADD ThresholdForHost int not null default 500;
   ALTER TABLE t_Zone_Info ADD PeriodForHost int not null default 60;
END;