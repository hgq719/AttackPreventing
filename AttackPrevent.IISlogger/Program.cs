using log4net;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AttackPrevent.IISlogger
{
    class Program
    {
        const String SessionName = "iis-etw";
        private static ConcurrentBag<byte[]> etwDataList = new ConcurrentBag<byte[]>();
        private static string apiUrl = string.Empty;
        static void Main(string[] args)
        {
            apiUrl = ConfigurationManager.AppSettings["iisLogApiUrl"];
            LogManager.GetLogger("").Info(apiUrl);
            ServicePointManager.DefaultConnectionLimit = 100;

            //Thread thread = new Thread(new ThreadStart(SendData));
            //thread.Start();


            Timer timer = new Timer(new TimerCallback(SendData2), null, 0, 1000);

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
        private static void OnIISRequest(TraceEvent request)
        {
            etwDataList.Add(request.EventData());
            //Console.WriteLine(request.ToString());
        }

        private static async void HttpPost(string Url, byte[] postData)
        {
            System.GC.Collect();
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.KeepAlive = false;
            request.Method = "POST";
            request.Accept = "*/*";
            request.ContentType = "application/octet-stream";
            request.ContentLength = postData.LongLength;
            //request.CookieContainer = cookie;
            Stream myRequestStream = request.GetRequestStream();
            await myRequestStream.WriteAsync(postData, 0, postData.Length);
            myRequestStream.Close();
            if (request != null)
            {
                request.Abort();
                //request = null;
            }
        }

        private static void SendData()
        {
            //ServicePointManager.DefaultConnectionLimit = 100;
            while (true)
            {
                if (etwDataList.Count > 0)
                {
                    try
                    {
                        byte[] postData = Serialize(etwDataList);
                        Console.WriteLine($"{DateTime.Now.ToString()} -  {etwDataList.Count}");
                        var newBag = new ConcurrentBag<byte[]>();
                        Interlocked.Exchange<ConcurrentBag<byte[]>>(ref etwDataList, newBag);
                        HttpPost(apiUrl, postData);
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("").Error(ex);
                    }
                    //finally
                    //{
                    //    etwDataList.Clear();
                    //}
                }

                Thread.Sleep(1000);
            }
        }

        private static void SendData2(object obj)
        {
            if (etwDataList.Count > 0)
            {
                try
                {
                    byte[] postData = Serialize(etwDataList);
                    int postCount = etwDataList.Count;
                    var newBag = new ConcurrentBag<byte[]>();
                    Interlocked.Exchange<ConcurrentBag<byte[]>>(ref etwDataList, newBag);
                    HttpPost(apiUrl, postData);
                    Console.WriteLine($"{DateTime.Now.ToString()} -  {postCount}");
                }
                catch (Exception ex)
                {
                    LogManager.GetLogger("").Error(ex);
                }
            }

        }

        public static byte[] Serialize(ConcurrentBag<byte[]> data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            MemoryStream rems = new MemoryStream();
            formatter.Serialize(rems, data);
            return rems.GetBuffer();
        }
    }
}
