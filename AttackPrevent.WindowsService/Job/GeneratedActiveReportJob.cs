using AttackPrevent.Business;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.WindowsService.Job
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
