declare @constraintName varchar(max)

select @constraintName = b.name from syscolumns a,sysobjects b where a.id=object_id('t_Zone_Info') 
and b.id=a.cdefault and a.name='HostNames' and b.name like 'DF%'

IF NOT EXISTS
(
SELECT 1 where @constraintName is not null 
)
BEGIN
ALTER TABLE t_Zone_Info ADD HostNames varchar(max) not null DEFAULT ('');
END;