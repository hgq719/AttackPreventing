using AttackPrevent.Business;
using System;

namespace AttackPrevent.EmailService.Job
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
