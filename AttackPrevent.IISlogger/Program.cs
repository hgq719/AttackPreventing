using log4net;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Concurrent;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace AttackPrevent.IISlogger
{
    class Program
    {
        const String SessionName = "iis-etw";
        private static ConcurrentBag<byte[]> _etwDataList = new ConcurrentBag<byte[]>();
        private static string _apiUrl = string.Empty;

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
                //request = null;
            }
        }

        #region Old Version

        //private static void SendData()
        //{
        //    //ServicePointManager.DefaultConnectionLimit = 100;
        //    while (true)
        //    {
        //        if (_etwDataList.Count > 0)
        //        {
        //            try
        //            {
        //                byte[] postData = Serialize(_etwDataList);
        //                Console.WriteLine($"{DateTime.Now.ToString()} -  {_etwDataList.Count}");
        //                var newBag = new ConcurrentBag<byte[]>();
        //                Interlocked.Exchange<ConcurrentBag<byte[]>>(ref _etwDataList, newBag);
        //                HttpPost(_apiUrl, postData);
        //            }
        //            catch (Exception ex)
        //            {
        //                LogManager.GetLogger("").Error(ex);
        //            }
        //            //finally
        //            //{
        //            //    etwDataList.Clear();
        //            //}
        //        }

        //        Thread.Sleep(1000);
        //    }
        //}

        #endregion


        private static void SendData(object obj)
        {
            if (_etwDataList.Count <= 0) return;
            try
            {
                var postData = Serialize(_etwDataList);
                var postCount = _etwDataList.Count;
                var newBag = new ConcurrentBag<byte[]>();
                Interlocked.Exchange(ref _etwDataList, newBag);
                HttpPost(_apiUrl, postData);
                Console.WriteLine($"{DateTime.Now.ToString(CultureInfo.InvariantCulture)} -  {postCount}");
            }
            catch (Exception ex)
            {
                LogManager.GetLogger(string.Empty).Error(ex);
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
