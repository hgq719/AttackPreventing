using AttackPrevent.Business;
using System;

namespace AttackPrevent.EmailService.Job
{
    public class GeneratedActiveReportJob : BaseJob
    {
        protected override void doWork()
        {
            logService.Debug(DateTime.Now);
            IActiveReportService activeReportService = ActiveReportService.GetInstance();
            activeReportService.GeneratedActiveReport();
        }
    }
}
