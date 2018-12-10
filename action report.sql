create table t_Action_Report(
Id int identity primary key,
Title nvarchar(512) not null default(''),
ZoneId nvarchar(256) not null default(''),
IP nvarchar(256) not null default(''),
HostName nvarchar(256) not null default(''),
[Max] int not null default(0), 
[Min] int not null default(0), 
[Avg] int not null default(0), 
FullUrl nvarchar(max) not null default(''),
CreatedTime datetime not null default(sysdatetime()),
Mode nvarchar(256) not null default(''),
[Count] int not null default(0), 
MaxDisplay nvarchar(256) not null default(''),
MinDisplay nvarchar(256) not null default(''),
AvgDisplay nvarchar(256) not null default(''),
Remark nvarchar(512) not null default('')
);

create table t_Smtp_Queue(
Id int identity primary key,
Title nvarchar(512) not null default(''),
[Status] int not null default(0), 
CreatedTime datetime not null default(sysdatetime()),
SendedTime datetime not null default(sysdatetime()),
Remark nvarchar(512) not null default('')
);