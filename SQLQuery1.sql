sp_help  t_logs;

drop index PK_t_Logs on t_logs;

select * from t_logs with (nolock) where ZoneTableId = 17 and len(detail) > 10000
and LogTime >  GETUTCDATE() -1 and LogLevel in (0,1,2)

select GETUTCDATE() -1
