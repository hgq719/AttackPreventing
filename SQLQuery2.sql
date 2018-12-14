		SELECT [ID], [ZoneId], [IP], [LatestTriggerTime], [RuleId], [Remark] 
		FROM T_Ban_IP_History WITH(NOLOCK) 
		WHERE ZoneId = @ZoneId

		delete from T_Ban_IP_History;
		delete from t_Logs;

		select * from t_Zone_Info;


		delete from t_Zone_Info where ZoneName = 'shit';


		select * from T_Ban_IP_History;