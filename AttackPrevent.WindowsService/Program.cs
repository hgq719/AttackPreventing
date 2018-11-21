using AttackPrevent.WindowsService.Job;
using AttackPrevent.WindowsService.SysConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AttackPrevent.WindowsService
{
    class Program
    {
        static void Main(string[] args)
        {
            new LogAnalyzeJob().Execute(null);
            //return;
            if (args.Length != 0)
            {
                WindowsServiceInstaller srv = new WindowsServiceInstaller();
                switch (args[0].ToLower())
                {
                    case "/install":
                        Install(srv);
                        return;
                    case "/uninstall":
                        Uninstall(srv);
                        return;
                    case "/debug":
                        Debug();
                        return;
                    default:
                        Help();
                        return;
                }
            }
            else
            {
                Start();
            }
        }

        #region 服务相关
        private const string SrvName = "Comm100.AcctackLogAnalyzeService";
        private const string SrvDisplay = "Comm100.AcctackLogAnalyzeService";
        private const String SrvDescription = "Acctack Log Analyze windodws Service for Comm100";
        #endregion

        #region 方法定义
        private static void Install(WindowsServiceInstaller srv)
        {
            bool ok = srv.InstallService(System.Reflection.Assembly.GetExecutingAssembly().Location, SrvName, SrvDisplay, SrvDescription);

            if (ok)
                Console.WriteLine("Service installed.");
            else
                Console.WriteLine("There was a problem with installation.");
        }

        private static void Uninstall(WindowsServiceInstaller srv)
        {
            bool ok = srv.UnInstallService(SrvDisplay);

            if (ok)
                Console.WriteLine("Service uninstalled.");
            else
                Console.WriteLine("There was a problem with uninstallation.");
        }


        private static void Debug()
        {
            new AnalyzeService().Start();
            Console.ReadLine();
        }

        private static void Help()
        {
            Console.WriteLine("Command line parameters:");

            Console.WriteLine("\t/install\tInstalls the service");

            Console.WriteLine("\t/uninstall\tUnstalls the service");

            Console.WriteLine("\t/debug\tDebug the service");

            Console.ReadLine();
        }

        private static void Start()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                    new AnalyzeService()
            };
            ServiceBase.Run(ServicesToRun);
        }
        #endregion
    }
}
