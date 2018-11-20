using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface ILogService
    {
        void Debug(object message);
        void Error(object message);
    }
    public class LogService:ILogService
    {
        private ILog logger = LogManager.GetLogger("WebLogger");

        public void Debug(object message)
        {
            logger.Debug(message);
        }

        public void Error(object message)
        {
            logger.Error(message);
        }
    }
}
