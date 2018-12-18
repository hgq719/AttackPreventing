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
        void Info(object message);
        void Error(object message);

    }
    public class LogService:ILogService
    {
        private readonly ILog _logger = LogManager.GetLogger("WebLogger");

        public void Debug(object message)
        {
            _logger.Debug(message);
        }

        public void Info(object message)
        {
            _logger.Info(message);
        }

        public void Error(object message)
        {
            _logger.Error(message);
        }
    }
}
