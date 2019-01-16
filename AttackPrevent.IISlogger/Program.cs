
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;

using log4net;
using log4net.Core;

namespace AttackPrevent.IISlogger
{
    class Program
    {
        private const string SessionName = "iis-etw";
        private static ConcurrentBag<byte[]> _etwDataList = new ConcurrentBag<byte[]>();
        private static string _apiUrl = string.Empty;
        private static readonly ILog Loger = LogManager.GetLogger("Program");

        static void Main()
        {
            _apiUrl = ConfigurationManager.AppSettings["iisLogApiUrl"];
            LogManager.GetLogger(string.Empty).Info(_apiUrl);
            ServicePointManager.DefaultConnectionLimit = 100;

            var timer = new Timer(new TimerCallback(SendData), null, 0, 1000);

            // create a new real-time ETW trace session
            using (var session = new TraceEventSession(SessionName))
            {
                // enable IIS ETW provider and set up a new trace source on it
                session.EnableProvider("Microsoft-Windows-IIS-Logging", TraceEventLevel.Verbose);

                using (var traceSource = new ETWTraceEventSource(SessionName, TraceEventSourceType.Session))
                {
                    Console.WriteLine("Session started, listening for events...");
                    var parser = new DynamicTraceEventParser(traceSource);
                    parser.All += OnIISRequest;

                    traceSource.Process();
                    Console.ReadLine();
                    traceSource.StopProcessing();
                }
            }
        }
        // ReSharper disable once InconsistentNaming
        private static void OnIISRequest(TraceEvent request)
        {
            _etwDataList.Add(request.EventData());
#if DEBUG
            Console.WriteLine(request.ToString());
#endif
        }

        private static async void HttpPost(string url, byte[] postData)
        {
            GC.Collect();
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;
            request.Method = "POST";
            request.Accept = "*/*";
            request.ContentType = "application/octet-stream";
            request.ContentLength = postData.LongLength;
            //request.CookieContainer = cookie;
            var myRequestStream = request.GetRequestStream();
            await myRequestStream.WriteAsync(postData, 0, postData.Length);
            myRequestStream.Close();
            if (request != null)
            {
                request.Abort();
            }
        }

        private static void SendData(object obj)
        {
            if (_etwDataList.Count <= 0)
            {
                Loger.Info("No need to send data. there are no data in etwDataList.");
                return;
            } 
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var postCount = _etwDataList.Count;
                var newBag = new ConcurrentBag<byte[]>();
                var postData = Serialize(_etwDataList);

                Interlocked.Exchange(ref _etwDataList, newBag);

                HttpPost(_apiUrl, postData);
                stopwatch.Stop();
                LogManager.GetLogger(string.Empty).Info($"time cost: {stopwatch.Elapsed}, post count: {postCount}, etwData now count: {_etwDataList.Count}");
                Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} -  {postCount}");
            }
            catch (Exception ex)
            {
                Loger.Error($"Error when sending data, error message = {ex.Message} \n stacktrace = {ex.StackTrace}.");
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
