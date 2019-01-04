delete from t_logs where ZoneTableId = 17 


and LogTime >  GETUTCDATE() -1 and LogLevel in (0,1,2) 

 truncate table t_Ban_IP_History

