using log4net;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.IISLogger.WindowsService
{
    public partial class Service1 : ServiceBase
    {
        const string SessionName = "iis-etw";
        private static ConcurrentBag<byte[]> _etwDataList = new ConcurrentBag<byte[]>();
        private static string _apiUrl = string.Empty;
        private static string _apiKey = string.Empty;
        private static Timer _timer;
        private static readonly ILog Loger = LogManager.GetLogger("Program");

        private static Thread thread;

        public Service1()
        {
            InitializeComponent();

            try
            {
                _apiUrl = ConfigurationManager.AppSettings["iisLogApiUrl"];
                _apiKey = ConfigurationManager.AppSettings["iisLogApiKey"];
                Loger.Info(_apiUrl);
                ServicePointManager.DefaultConnectionLimit = 100;
            }
            catch (Exception ex)
            {
                Loger.Info(ex);
            }
        }

        protected override void OnStart(string[] args)
        {
                _timer = new Timer(new TimerCallback(SendData), null, 0, 1000);

                thread = new Thread(new ThreadStart(delegate
                {
                    try
                    {
                        using (var session = new TraceEventSession(SessionName))
                        {
                            // enable IIS ETW provider and set up a new trace source on it
                            session.EnableProvider("Microsoft-Windows-IIS-Logging", TraceEventLevel.Verbose);

                            using (var traceSource = new ETWTraceEventSource(SessionName, TraceEventSourceType.Session))
                            {
                                Loger.Info("Session started, listening for events...");
                                var parser = new DynamicTraceEventParser(traceSource);
                                parser.All += OnIISRequest;

                                traceSource.Process();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Loger.Info(ex);
                    }
                    
                }));
            thread.Start();

        }

        protected override void OnStop()
        {
            try
            {
                if (thread != null)
                {
                    thread.Abort();
                }
                _timer.Dispose();
            }
            catch (Exception ex)
            {
                Loger.Info(ex);
            }


        }

        private static void OnIISRequest(TraceEvent request)
        {
            _etwDataList.Add(request.EventData());
#if DEBUG
            //Console.WriteLine(request.ToString());
            //ETWPrase eTWPrase = new ETWPrase(request.EventData());
            //Console.WriteLine(eTWPrase.Cs_uri_stem);
#endif
        }

        private static void HttpPost(string url, byte[] postData)
        {
            try
            {
                GC.Collect();
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.KeepAlive = false;
                request.Method = "POST";
                request.Accept = "*/*";
                request.ContentType = "application/octet-stream";
                request.Headers.Add("Authorization", _apiKey);
                request.ContentLength = postData.LongLength;
                request.Timeout = 500;
                //request.CookieContainer = cookie;
                var myRequestStream = request.GetRequestStream();
                myRequestStream.Write(postData, 0, postData.Length);
                myRequestStream.Close();
                //if (request != null)
                //{
                //    request.Abort();
                //    //request = null;
                //}
            }
            catch (Exception ex)
            {
                Loger.Error($"Error when post data to remote server. error message: {ex.Message}.\n stack trace: {ex.StackTrace}.");
            }

        }

        private static int sendCount = 0;
        private static void SendData(object obj)
        {
            //if (_etwDataList.Count <= 0)
            //{
            //    //Loger.Info("No Data need to be sent.");
            //    return;
            //} 
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                var postCount = _etwDataList.Count;
                var newBag = new ConcurrentBag<byte[]>();
                var postData = Serialize(_etwDataList);

                Interlocked.Exchange(ref _etwDataList, newBag);

                HttpPost(_apiUrl, postData);
                stopwatch.Stop();
                sendCount++;
                LogManager.GetLogger(string.Empty).Info($"time cost: {stopwatch.Elapsed}, post count: {postCount}, etwData now count: {_etwDataList.Count}, sendCount: {sendCount}");
                Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} - postCount: {postCount}, sendCount: {sendCount}");
            }
            catch (Exception ex)
            {
                Loger.Error($"Error when sending data. error message: {ex.Message}.\n stack trace: {ex.StackTrace}.");
            }
        }

        private static byte[] Serialize(ConcurrentBag<byte[]> data)
        {
            var formatter = new BinaryFormatter();
            var rems = new MemoryStream();
            formatter.Serialize(rems, data);
            return rems.GetBuffer();
        }
    }
}
