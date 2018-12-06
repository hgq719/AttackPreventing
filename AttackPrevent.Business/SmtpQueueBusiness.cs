using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AttackPrevent.Business
{
    public class SmtpQueueBusiness
    {
        public static List<SmtpQueue> GetList()
        {
            return SmtpQueueAccess.GetList();
        }

        public static SmtpQueue GetByTitle(string title)
        {
            return SmtpQueueAccess.GetByTitle(title);
        }

        public static void Add(SmtpQueue item)
        {
            SmtpQueueAccess.Add(item);
        }

        public static void Edit(SmtpQueue item)
        {
            SmtpQueueAccess.Edit(item);
        }

        public static void Delete(string title)
        {
            SmtpQueueAccess.Delete(title);
        }
                
    }
}
