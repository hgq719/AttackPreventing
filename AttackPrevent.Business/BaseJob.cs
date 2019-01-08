using AttackPrevent.Business;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class BaseJob : IJob
    {
        protected ILogService logService = new LogService();
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                doWork();
            }
            catch (Exception e)
            {
                logService.Error(e.StackTrace);
            }
            finally
            {

            }

            await Task.FromResult(0);
        }
        protected virtual void doWork()
        {
            
        }
    }
}
