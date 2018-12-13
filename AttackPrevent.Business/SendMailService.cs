using AttackPrevent.Model;
using AttackPrevent.Model.Cloudflare;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.Business
{
    public interface ISendMailService
    {
        void MainQueueDoWork();
    }
    public class SendMailService : ISendMailService
    {
        private readonly string nothing = "Not Applicable";
        private readonly ILogService logService = new LogService();

        private static object obj_Sync = new object();
        private static ISendMailService sendMailService;
        private bool isProcessing = false;

        private SendMailService()
        {
        }
        public static ISendMailService GetInstance()
        {
            if (sendMailService == null)
            {
                lock (obj_Sync)
                {
                    sendMailService = new SendMailService();
                }
            }

            return sendMailService;
        }

        /// <summary>
        /// 发送邮件
        /// </summary>
        public void MainQueueDoWork()
        {
            try
            {
                if (!isProcessing)
                {
                    isProcessing = true;
                    List<SmtpQueue> smtpQueues = SmtpQueueBusiness.GetList();
                    List<SmtpQueue> subQueues = smtpQueues.Where(a => a.Status == 0).ToList();
                    if (subQueues != null && subQueues.Count > 0)
                    {
                        var configuration = GlobalConfigurationBusiness.GetConfigurationList().FirstOrDefault();
                        for (int i = 0; i < subQueues.Count; i++)
                        {
                            SmtpQueue smtp = subQueues[i];
                            string title = smtp.Title;
                            string bodys = GeneratedMail(title);
                            string smtpserver = ConstValues.emailsmtp;
                            bool enablessl = ConstValues.emailssl;
                            string userName = ConstValues.emailusername;
                            string pwd = Utils.AesDecrypt(ConstValues.emailpassword);
                            string nickName = ConstValues.emailnickname;
                            string strfrom = ConstValues.emailfrom;
                            string strto = configuration.EmailAddForWhiteList;
                            string subj = string.Format("IP Action Report {0}", title);
                            int port = ConstValues.emailport;
                            bool authentication = ConstValues.emailauthentication;
                            int timeout = ConstValues.emailtimeout;

                            Utils.SendMail(smtpserver, enablessl, userName, pwd, nickName, strfrom, strto, subj, bodys, port, authentication, timeout);

                            smtp.Status = 1;
                            SmtpQueueBusiness.Edit(smtp);
                        }
                    }
                    isProcessing = false;
                }
            }
            catch (Exception e)
            {
                logService.Error(e);
                isProcessing = false;
            }
            finally
            {

            }

        }

        private string GeneratedMail(string title)
        {
            StringBuilder mail = new StringBuilder();
            SmtpQueue smtpQueue = SmtpQueueBusiness.GetByTitle(title);
            if (smtpQueue != null && smtpQueue.Id > 0)
            {
                mail.AppendLine("<div id=\"mail\">");
                List<ActionReport> actionReports = ActionReportBusiness.GetListByTitle(title);

                List<ZoneEntity> zoneEntities = ZoneBusiness.GetZoneList().Where(a => a.IfEnable).ToList();
                foreach (ZoneEntity zone in zoneEntities)
                {
                    List<ActionReport> subActionReports = actionReports.Where(a => a.ZoneId == zone.ZoneId).ToList();
                    string body = CreateMainZone(zone.ZoneName, subActionReports);
                    mail.Append(body);
                }
                mail.AppendLine("</div>");
            }
            return mail.ToString();
        }
        private string CreateMainZone(string zoneName, List<ActionReport> actionReports)
        {
            StringBuilder mail = new StringBuilder();
            mail.AppendLine("<div>");
            mail.AppendFormat("<p style=\"margin-left:10px; \">zone:{0}</p>", zoneName);
            mail.AppendFormat("<table style=\"border: 1px solid #0094ff; width:98%; min-height: 25px; line-height: 25px; text-align: center; border-collapse: collapse; padding:2px; margin-left:10px;word-wrap:break-word; word-break:break-all;\">");
            mail.AppendFormat("<tr style=\"border: 1px solid #0094ff;\">");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:7%;\">IP</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:13%;\">Host name</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:10%;\">Max</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:10%;\">Min</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:10%;\">Avg.</th>");
            mail.AppendFormat("<th style=\"border: 1px solid #0094ff;width:50%;\">Full URLs</th>");
            mail.AppendLine("</tr>");
            foreach (ActionReport actionReport in actionReports)
            {
                string ip = actionReport.IP;
                string hostName = actionReport.HostName;
                string max = actionReport.MaxDisplay;
                string min = actionReport.MinDisplay;
                string avg = actionReport.AvgDisplay;
                string fullUrls = actionReport.FullUrl;

                List<string> top5UrlList = JsonConvert.DeserializeObject<List<string>>(fullUrls);

                string color = GetBackgroundColor(actionReport);
                mail.AppendFormat("<tr style=\"border: 1px solid #0094ff;{0}\">", color);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", ip);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", hostName);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", max);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", min);
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", avg);
                string urls = "";
                foreach (string url in top5UrlList)
                {
                    urls += string.Format("<a style=\"text-decoration:none; color:#000000;\" href=\"javascript:void();\">{0}</a><br>", url);
                }
                mail.AppendFormat("<td style=\"border: 1px solid #0094ff;text-align:left;\">{0}</td>", urls);
                mail.AppendLine("</tr>");
            }
            mail.AppendLine("</table>");
            mail.AppendLine("</div>");
            return mail.ToString();
        }
        private string GetBackgroundColor(ActionReport actionReport)
        {
            string color = "";
            if (actionReport.MaxDisplay.Contains("Not Applicable") ||
                actionReport.MinDisplay.Contains("Not Applicable") ||
                actionReport.AvgDisplay.Contains("Not Applicable"))
            {
                color = "background-color:red;";
            }
            else
            {
                if (actionReport.MaxDisplay.Contains("("))
                {
                    string[] vls = actionReport.MaxDisplay.Replace(")", "").Split('(');
                    int firNum = Convert.ToInt32(vls[0]);
                    int lstNum = Convert.ToInt32(vls[1]);

                    if (firNum > lstNum)
                    {
                        color = "background-color:red;";
                    }
                }

                if (actionReport.AvgDisplay.Contains("("))
                {
                    string[] vls = actionReport.AvgDisplay.Replace(")", "").Split('(');
                    int firNum = Convert.ToInt32(vls[0]);
                    int lstNum = Convert.ToInt32(vls[1]);

                    if (firNum > lstNum)
                    {
                        color = "background-color:red;";
                    }
                }
            }
            return color;
        }

    }

}
