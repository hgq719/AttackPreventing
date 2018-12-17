using AttackPrevent.WindowsService.Job;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.WindowsService.SysConfig
{
    partial class AnalyzeService : ServiceBase
    {
        IScheduler scheduler;

        public AnalyzeService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            this.Start();
        }

        public void Start()
        {
            try
            {
                while (true)
                {
                    new LogAnalyzeJob().Execute();
                    Thread.Sleep(5000);
                    //var thread = new Thread();
                    //thread.Start(new LogAnalyzeJob().Execute(null));
                }
                ////Loger.Log("服务启动", logFileName);
                //try
                //{
                //    Task<IScheduler> taskScheduler;
                //    //初始化调度器工厂   
                //    ISchedulerFactory sf = new StdSchedulerFactory();
                //    //获取默认调度器   
                //    taskScheduler = sf.GetScheduler();
                //    scheduler = taskScheduler.Result;
                //    scheduler.Start();
                //}
                //catch (Exception )
                //{
                //    //Loger.Log(string.Format("服务执行失败,{0}", ex.Message), logFileName);
                //}
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        protected override void OnStop()
        {
            scheduler.Shutdown(true);
            //Loger.Log("服务停止", logFileName);
        }
    }
}
