using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public class ConstValues
    {
        /// <summary>
        /// STMP服务器
        /// </summary>
        public static readonly string emailsmtp = "smtp.qq.com";
        /// <summary>
        /// 是否启用SSL加密连接
        /// </summary>
        public static readonly bool emailssl = true;
        /// <summary>
        /// 邮箱账号
        /// </summary>
        public static readonly string emailusername = "394631316@qq.com";
        /// <summary>
        /// 邮箱密码
        /// </summary>
        public static readonly string emailpassword = "80WWxR/3ONunZJHnsY5uUA==";
        /// <summary>
        /// 发件人昵称
        /// </summary>
        public static readonly string emailnickname = "Comm100 Attack Prevent";
        /// <summary>
        /// 发件人地址
        /// </summary>
        public static readonly string emailfrom = "394631316@qq.com";
        /// <summary>
        /// 发送邮件超时时间
        /// </summary>
        public static readonly int emailtimeout = 90000;
        /// <summary>
        /// 发送邮件端口
        /// </summary>
        public static readonly int emailport = 25; 
        /// <summary>
        /// 是否启用用户名密码
        /// </summary>
        public static readonly bool emailauthentication = true;

        static ConstValues()
        {
            try
            {
                emailsmtp = ConfigurationManager.AppSettings["MailServer"];
                emailssl = Convert.ToBoolean(ConfigurationManager.AppSettings["MailServerIfSSL"]);
                emailusername = ConfigurationManager.AppSettings["MailServerUserName"];
                emailpassword = ConfigurationManager.AppSettings["MailServerPassword"];
                emailnickname = ConfigurationManager.AppSettings["MailServereNickName"];
                emailtimeout = Convert.ToInt32(ConfigurationManager.AppSettings["MailServerTimeout"]);
                emailport = Convert.ToInt32(ConfigurationManager.AppSettings["MailServerPort"]);
                emailauthentication = Convert.ToBoolean(ConfigurationManager.AppSettings["MailServerIfAuthentication"]);
                emailfrom = ConfigurationManager.AppSettings["MailServereFrom"];
            }
            catch(Exception e)
            {
                throw e;
            }
        }
    }
}
