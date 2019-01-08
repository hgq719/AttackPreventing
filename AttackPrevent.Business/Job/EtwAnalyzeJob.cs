using AttackPrevent.Business;
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
    public class EtwAnalyzeJob : BaseJob
    {
        protected override void doWork()
        {
            logService.Debug(DateTime.Now);
            IEtwAnalyzeService etwAnalyzeService = EtwAnalyzeService.GetInstance();
            etwAnalyzeService.doWork();
            logService.Debug("etwAnalyzeService start.");
        }
    }
}
