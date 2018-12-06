﻿using AttackPrevent.Access;
using AttackPrevent.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AttackPrevent.Business
{
    public class ActionReportBusiness
    {
        public static List<ActionReport> GetListByTitle(string title)
        {
            return ActionReportAccess.GetListByTitle(title);
        }

        public static List<ActionReport> GetListByIp(string ip)
        {
            return ActionReportAccess.GetListByIp(ip);
        }

        public static void Add(ActionReport item)
        {
            ActionReportAccess.Add(item);
        }

        public static void Edit(ActionReport item)
        {
            ActionReportAccess.Edit(item);
        }

        public static void Delete(string title)
        {
            ActionReportAccess.Delete(title);
        }

        public static int GetMaxForAction(string ip, string hostName)
        {
            List<ActionReport> actionReports = GetListByIp(ip);
            int? result = actionReports.OrderByDescending(a => a.Max).FirstOrDefault(a => a.Mode == "Action"&& a.HostName==hostName)?.Max;
            return result.HasValue ? result.Value : 0;
        }
        public static int GetMaxForWhiteList(string ip, string hostName)
        {
            List<ActionReport> actionReports = GetListByIp(ip);
            int? result = actionReports.OrderByDescending(a => a.Max).FirstOrDefault(a => a.Mode == "WhiteList" && a.HostName == hostName)?.Max;
            return result.HasValue ? result.Value : 0;
        }
        public static int GetMinForAction(string ip, string hostName)
        {
            List<ActionReport> actionReports = GetListByIp(ip);
            int? result = actionReports.OrderBy(a => a.Min).FirstOrDefault(a => a.Mode == "Action" && a.HostName == hostName)?.Min;
            return result.HasValue ? result.Value : 0;
        }
        public static int GetMinForWhiteList(string ip, string hostName)
        {
            List<ActionReport> actionReports = GetListByIp(ip);
            int? result = actionReports.OrderBy(a => a.Min).FirstOrDefault(a => a.Mode == "WhiteList" && a.HostName == hostName)?.Min;
            return result.HasValue ? result.Value : 0;
        }
        public static int GetAvgForAction(string ip, string hostName)
        {
            int result = -1;
            List<ActionReport> actionReports = GetListByIp(ip);
            int sum = actionReports.Where(a => a.Mode == "Action" && a.HostName == hostName).Sum(a => a.Avg);
            int count = actionReports.Where(a => a.Mode == "Action" && a.HostName == hostName).Count();
            if(count == 0)
            {
                result = 0;
            }
            else
            {
                result = sum / count;
            }
            return result;
        }
        public static int GetAvgForWhiteList(string ip, string hostName)
        {
            int result = -1;
            List<ActionReport> actionReports = GetListByIp(ip);
            int sum = actionReports.Where(a => a.Mode == "WhiteList" && a.HostName == hostName).Sum(a => a.Avg);
            int count = actionReports.Where(a => a.Mode == "WhiteList" && a.HostName == hostName).Count();
            if (count == 0)
            {
                result = 0;
            }
            else
            {
                result = sum / count;
            }
            return result;
        }
    }
}