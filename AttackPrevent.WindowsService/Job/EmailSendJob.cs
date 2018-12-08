﻿using AttackPrevent.Business;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.WindowsService.Job
{
    public class EmailSendJob : BaseJob
    {
        protected override void doWork()
        {
            logService.Debug(DateTime.Now);
            ISendMailService sendMailService = SendMailService.GetInstance();
            sendMailService.MainQueueDoWork();
        }
    }
}
