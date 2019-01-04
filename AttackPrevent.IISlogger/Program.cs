using log4net;
using Microsoft.Diagnostics.Tracing;
using Microsoft.Diagnostics.Tracing.Parsers;
using Microsoft.Diagnostics.Tracing.Session;
using System;
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
        const string SessionName = "iis-etw";
        private static List<byte[]> etwDataList = new List<byte[]>();
        private static string apiUrl = string.Empty;

        static void Main(string[] args)
        {
            apiUrl = ConfigurationManager.AppSettings["iisLogApiUrl"];
            LogManager.GetLogger("").Info(apiUrl);

            Thread thread = new Thread(new ThreadStart(SendData));
            thread.Start();

            CreateETWSession();
          
        }

        private static void CreateETWSession()
        {
            // create a new real-time ETW trace session
            using (var session = new TraceEventSession(SessionName))
            {
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
            etwDataList.Add(request.EventData());

            Console.WriteLine(request.ToString());
        }

        private static void HttpPost(string Url, byte[] postData)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "POST";
            request.Accept = "*/*";
            request.ContentType = "application/octet-stream";
            request.ContentLength = postData.LongLength;
            //request.CookieContainer = cookie;
            var myRequestStream = request.GetRequestStream();
            myRequestStream.Write(postData, 0, postData.Length);
            myRequestStream.Close();
        }

        private static void SendData()
        {
            while (true)
            {
                if (etwDataList.Count > 0)
                {
                    try
                    {
                        byte[] postData = Serialize(etwDataList);
                        HttpPost(apiUrl, postData);
                    }
                    catch (Exception ex)
                    {
                        LogManager.GetLogger("").Error(ex);
                    }
                    finally
                    {
                        etwDataList.Clear();
                    }
                }

                Thread.Sleep(1000);
            }
        }

        private static byte[] Serialize(List<byte[]> data)
        {
            var formatter = new BinaryFormatter();
            var rems = new MemoryStream();
            formatter.Serialize(rems, data);
            return rems.GetBuffer();
        }
    }
}
