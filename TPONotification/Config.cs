using System;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Configuration;
//using Tpo.Infrastructure.Logging;
using log4net;

namespace Tpo.Notification
{
    public static class Configure
    {
        static public string SMTPServer { get; set; }
        static public string SMSProviderDB { get; set; }
        static public string Tokens { get; set; }
        static public string LogPath { get; set; }
        static public string MessageQueueDB { get; set; }
        static public string LogDB { get; set; }
        static public string DBMQFileLocation { get; set; }

        static public int QueueId { get; set; }

        static public bool AsynMode { get; set; }
        static public bool LogCaller { get; set; }
        public static readonly ILog _Log;
        public static readonly SMS _sms; 

        static Configure()
        {
            // Cache all these values in static properties from the configure file.
            MessageQueueDB = ConfigurationManager.AppSettings["MessageQueueDB"];
            LogDB = ConfigurationManager.AppSettings["LogDB"];
            Tokens = ConfigurationManager.AppSettings["Tokens"];
            SMSProviderDB = ConfigurationManager.AppSettings["SMSProviderDB"];
            DBMQFileLocation = ConfigurationManager.AppSettings["MessageRootPath"];
            AsynMode = Boolean.Parse(ConfigurationManager.AppSettings["AsynMode"]);
            LogCaller = Boolean.Parse(ConfigurationManager.AppSettings["LogCaller"]);
            QueueId = int.Parse(ConfigurationManager.AppSettings["QueueId"]);
            XmlDocument doc = new XmlDocument();
            string LocationExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;

            SMTPServer = ConfigurationManager.AppSettings["SMTPSERVER"];
            LogPath = ConfigurationManager.AppSettings["LogRootPath"];

            if (string.IsNullOrEmpty(LogPath))
                LogPath = LocationExecutingAssembly;
            else
                LogPath = Path.Combine(LogPath, "LogData");

        //Tpo.Infrastructure.Configuration.Configurator.ConfigureFromSection("Tpo.Infrastructure");
            log4net.Config.XmlConfigurator.Configure();
            _Log = LogManager.GetLogger("sms");
            
            log4net.GlobalContext.Properties["host"] = Environment.MachineName;
            _sms = new SMS();
        }

        public static string getConfigByAttribute(XmlDocument doc, string attribute)
        {
            XmlNode node = doc.SelectSingleNode(string.Format("//@{0}", attribute));
            if (null != node)
                return node.InnerText;
            else
                return string.Empty;
        }

        public static string getConfigByNode(XmlDocument doc, string nodeName)
        {
            XmlNode node = doc.SelectSingleNode(string.Format("//{0}", nodeName));
            if (null != node)
                return node.InnerText;
            else
                return string.Empty;
        }

        public static void LogData(string message, Exception e)
        {
            string logFile = string.Format("{0}.{1}.txt", Configure.LogPath, Environment.MachineName);
            try
            {
                FileInfo fInfo = new FileInfo(logFile);
                if (fInfo.Exists && fInfo.Length > 1024 * 1024)
                    fInfo.Delete();
                if (null == e)
                {
                    //Log.Info(message);
                    _Log.Info(message);
                }
                else
                {
                    message = string.Format("{0}\n{1}{2}{1}", message, e.Message, e.StackTrace);
                    //Log.Error(message);
                    _Log.Error(message);
                }
                File.AppendAllText(logFile, string.Format("{0} - {1}\n",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss tt"), message));
            }
            catch
            {
            }
        }
    }
}
