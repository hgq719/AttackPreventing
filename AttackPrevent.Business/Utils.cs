using System;
using System.IO;
using System.Net.Mail;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AttackPrevent.Business
{
    public class Utils
    {
        private static string _secretKey = "2068c8964a4dcef78ee5103471a8db03";
        public static T GetMemoryCache<T>(string key)
        {
            MemoryCache cache = MemoryCache.Default;
            T t = (T)cache.Get(key);
            return t;
        }

        public static void SetMemoryCache<T>(string key, T value, int timeout = 30)
        {
            MemoryCache cache = MemoryCache.Default;
            CacheItemPolicy policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = new DateTimeOffset(DateTime.UtcNow.AddMinutes(timeout));
            cache.Set(key, value, policy);
        }

        public static void RemoveMemoryCache(string key)
        {
            MemoryCache cache = MemoryCache.Default;
            cache.Remove(key);
        }

        public static T GetMemoryCache<T>(string key, Func<T> func, int timeout = 30)
        {
            T t = GetMemoryCache<T>(key);
            if (t == null)
            {
                t = func();
                SetMemoryCache(key, t, timeout);
            }

            return t;
        }

        public static string AesEncrypt(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            var toEncryptArray = Encoding.UTF8.GetBytes(str);

            var rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(_secretKey),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = rm.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray);
        }

        public static string AesDecrypt(string str)
        {
            if (string.IsNullOrEmpty(str)) return null;
            byte[] toEncryptArray = Convert.FromBase64String(str);

            var rm = new RijndaelManaged
            {
                Key = Encoding.UTF8.GetBytes(_secretKey),
                Mode = CipherMode.ECB,
                Padding = PaddingMode.PKCS7
            };

            var cTransform = rm.CreateDecryptor();
            var resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Encoding.UTF8.GetString(resultArray);
        }
        public static bool IsValidIp(string ip)
        {
            return Regex.IsMatch(ip, @"^(([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])\.){3}([0-9]|[1-9][0-9]|1[0-9]{2}|2[0-4][0-9]|25[0-5])$");
        }
        public static string GetFileContext(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (StreamReader sr = new StreamReader(filePath))
                {
                    return sr.ReadToEnd();
                }
            }
            else
            {
                throw new Exception("文件不存在!");
            }
        }
        #region 发送电子邮件
        /// <summary>
        /// 发送电子邮件
        /// </summary>
        /// <param name="smtpserver">SMTP服务器</param>
        /// <param name="enablessl">是否启用SSL加密</param>
        /// <param name="userName">登录帐号</param>
        /// <param name="pwd">登录密码</param>
        /// <param name="nickName">发件人昵称</param>
        /// <param name="strfrom">发件人</param>
        /// <param name="strto">收件人</param>
        /// <param name="subj">主题</param>
        /// <param name="bodys">内容</param>
        public static void SendMail(string smtpserver, bool enablessl, string userName, string pwd, string nickName, string strfrom, string strto, string subj, string bodys, int port, bool authentication, int timeout)
        {
            SmtpClient _smtpClient = new SmtpClient();
            _smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;//指定电子邮件发送方式
            _smtpClient.Host = smtpserver;//指定SMTP服务器
            if (authentication)
            {
                _smtpClient.Credentials = new System.Net.NetworkCredential("testaccount", pwd);//用户名和密码
            }
            _smtpClient.Timeout = timeout;
            _smtpClient.Port = port;
            if (enablessl == true)
            {
                _smtpClient.EnableSsl = true;
            }

            MailAddress _from = new MailAddress(strfrom, nickName);
            //MailAddress _to = new MailAddress(strto);
            //MailMessage _mailMessage = new MailMessage(_from, _to);
            MailMessage _mailMessage = new MailMessage();
            _mailMessage.From = _from;//电子邮件的发件人
            _mailMessage.Subject = subj;//主题
            _mailMessage.Body = bodys;//内容
            _mailMessage.BodyEncoding = System.Text.Encoding.Default;//正文编码
            _mailMessage.IsBodyHtml = true;//设置为HTML格式
            _mailMessage.Priority = MailPriority.Normal;//优先级

            //遍历收件人邮箱地址，并添加到此邮件的收件人里       
            if (strto.Length != 0)
            {
                string[] receivers = strto.Split(';');
                for (int i = 0; i < receivers.Length; i++)
                {
                    if (receivers[i].Length > 0)
                    {
                        _mailMessage.To.Add(receivers[i]);//为该电子邮件添加联系人
                    }
                }
            }

            _smtpClient.Send(_mailMessage);
        }
        #endregion
    }
}
