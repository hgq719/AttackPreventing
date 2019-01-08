using AttackPrevent.Business;
using AttackPrevent.Business.Cloundflare;
using Quartz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class CloudflareLogHandleJob : BaseJob
    {
        protected override void doWork()
        {
            logService.Debug(DateTime.Now);
            IBackgroundTaskService backgroundTaskService = BackgroundTaskService.GetInstance();
            backgroundTaskService.doWork();
            logService.Debug("CloudflareLogHandleService start.");
        }
    }
}
