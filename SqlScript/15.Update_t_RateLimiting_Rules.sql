declare @constraintName varchar(200)

select @constraintName = b.name from syscolumns a,sysobjects b where a.id=object_id('t_RateLimiting_Rules') 
and b.id=a.cdefault and a.name='EnlargementFactor' and b.name like 'DF%'

IF EXISTS
(
SELECT 1 where @constraintName is not null 
)
BEGIN
exec('alter table t_RateLimiting_Rules drop constraint '+ @constraintName);
ALTER TABLE t_RateLimiting_Rules ALTER column EnlargementFactor numeric(6,2) not null;
ALTER TABLE t_RateLimiting_Rules ADD DEFAULT (0.00) FOR EnlargementFactor WITH VALUES;
END;