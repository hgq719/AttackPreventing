using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Runtime.Caching;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

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
                _smtpClient.Credentials = new System.Net.NetworkCredential(userName, pwd);//用户名和密码
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
        public static List<string> ReadFileToList(string path)
        {
            List<string> list = new List<string>();

            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (StreamReader sr = new StreamReader(fs, Encoding.Default))
            {
                while (!sr.EndOfStream)
                {
                    string strLine = sr.ReadLine();
                    list.Add(strLine);
                }
            }

            return list;
        }
        public static List<byte[]> Deserialize(byte[] buff)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using(MemoryStream stream = new MemoryStream(buff))
            {
                return formatter.Deserialize(stream) as List<byte[]>;
            }
        }
        public static readonly string Separator = ",";

        #region 获取浏览器版本号

        /// <summary>
        /// 获取浏览器版本号
        /// </summary>
        /// <returns></returns>
        public static string GetBrowser()
        {
            HttpBrowserCapabilities bc = HttpContext.Current.Request.Browser;
            return bc.Browser + bc.Version;
        }

        #endregion

        #region 获取操作系统版本号

        /// <summary>
        /// 获取操作系统版本号
        /// </summary>
        /// <returns></returns>
        public static string GetOSVersion()
        {
            //UserAgent 
            var userAgent = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"];

            var osVersion = "未知";

            if (userAgent.Contains("NT 6.1"))
            {
                osVersion = "Windows 7";
            }
            else if (userAgent.Contains("NT 6.0"))
            {
                osVersion = "Windows Vista/Server 2008";
            }
            else if (userAgent.Contains("NT 5.2"))
            {
                osVersion = "Windows Server 2003";
            }
            else if (userAgent.Contains("NT 5.1"))
            {
                osVersion = "Windows XP";
            }
            else if (userAgent.Contains("NT 5"))
            {
                osVersion = "Windows 2000";
            }
            else if (userAgent.Contains("NT 4"))
            {
                osVersion = "Windows NT4";
            }
            else if (userAgent.Contains("Me"))
            {
                osVersion = "Windows Me";
            }
            else if (userAgent.Contains("98"))
            {
                osVersion = "Windows 98";
            }
            else if (userAgent.Contains("95"))
            {
                osVersion = "Windows 95";
            }
            else if (userAgent.Contains("Mac"))
            {
                osVersion = "Mac";
            }
            else if (userAgent.Contains("Unix"))
            {
                osVersion = "UNIX";
            }
            else if (userAgent.Contains("Linux"))
            {
                osVersion = "Linux";
            }
            else if (userAgent.Contains("SunOS"))
            {
                osVersion = "SunOS";
            }
            return osVersion;
        }
        #endregion

        #region 获取客户端IP地址

        /// <summary>
        /// 获取客户端IP地址
        /// </summary>
        /// <returns></returns>
        public static string GetIP()
        {
            string result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            if (string.IsNullOrEmpty(result))
            {
                result = HttpContext.Current.Request.UserHostAddress;
            }
            if (string.IsNullOrEmpty(result))
            {
                return "0.0.0.0";
            }
            return result;
        }

        #endregion

        #region 取客户端真实IP

        ///  <summary>  
        ///  取得客户端真实IP。如果有代理则取第一个非内网地址  
        ///  </summary>  
        public static string GetIPAddress()
        {

            var result = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(result))
            {
                //可能有代理  
                if (result.IndexOf(".") == -1)        //没有“.”肯定是非IPv4格式  
                    result = null;
                else
                {
                    if (result.IndexOf(",") != -1)
                    {
                        //有“,”，估计多个代理。取第一个不是内网的IP。  
                        result = result.Replace("  ", "").Replace("'", "");
                        string[] temparyip = result.Split(",;".ToCharArray());
                        for (int i = 0; i < temparyip.Length; i++)
                        {
                            if (IsIPAddress(temparyip[i])
                                    && temparyip[i].Substring(0, 3) != "10."
                                    && temparyip[i].Substring(0, 7) != "192.168"
                                    && temparyip[i].Substring(0, 7) != "172.16.")
                            {
                                return temparyip[i];        //找到不是内网的地址  
                            }
                        }
                    }
                    else if (IsIPAddress(result))  //代理即是IP格式  
                        return result;
                    else
                        result = null;        //代理中的内容  非IP，取IP  
                }

            }

            string IpAddress = (HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != null && HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] != String.Empty) ? HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"] : HttpContext.Current.Request.ServerVariables["HTTP_X_REAL_IP"];

            if (string.IsNullOrEmpty(result))
                result = HttpContext.Current.Request.ServerVariables["HTTP_X_REAL_IP"];

            if (string.IsNullOrEmpty(result))
                result = HttpContext.Current.Request.UserHostAddress;

            return result;
        }

        #endregion

        #region  判断是否是IP格式

        ///  <summary>
        ///  判断是否是IP地址格式  0.0.0.0
        ///  </summary>
        ///  <param  name="str1">待判断的IP地址</param>
        ///  <returns>true  or  false</returns>
        public static bool IsIPAddress(string str1)
        {
            if (string.IsNullOrEmpty(str1) || str1.Length < 7 || str1.Length > 15) return false;

            const string regFormat = @"^d{1,3}[.]d{1,3}[.]d{1,3}[.]d{1,3}$";

            var regex = new Regex(regFormat, RegexOptions.IgnoreCase);
            return regex.IsMatch(str1);
        }

        #endregion

        #region 获取公网IP
        /// <summary>
        /// 获取公网IP
        /// </summary>
        /// <returns></returns>
        public static string GetNetIP()
        {
            string tempIP = "";
            try
            {
                System.Net.WebRequest wr = System.Net.WebRequest.Create("http://city.ip138.com/ip2city.asp");
                System.IO.Stream s = wr.GetResponse().GetResponseStream();
                System.IO.StreamReader sr = new System.IO.StreamReader(s, System.Text.Encoding.GetEncoding("gb2312"));
                string all = sr.ReadToEnd(); //读取网站的数据

                int start = all.IndexOf("[") + 1;
                int end = all.IndexOf("]", start);
                tempIP = all.Substring(start, end - start);
                sr.Close();
                s.Close();
            }
            catch
            {
                if (System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList.Length > 1)
                    tempIP = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).AddressList[1].ToString();
                if (string.IsNullOrEmpty(tempIP))
                    return GetIP();
            }
            return tempIP;
        }
        #endregion

    }
}
