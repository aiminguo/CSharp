using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace Tpo.Notification
{
    public class Handler : TPOMSGCLIENTLib.DBMsg
    {
        private XmlDocument _doc = new XmlDocument();
        public void Process(string message)
        {
            try
            {
                log4net.GlobalContext.Properties["time"] = DateTime.Now;
                if (message.Trim().EndsWith(".dbm", StringComparison.OrdinalIgnoreCase))
                {
                    _doc.Load(message); 
                }
                else
                {
                    _doc.LoadXml(message);
                }
                TPONotification tpo = new TPONotification();
                string qmMessage = Configure.getConfigByNode(_doc, "qm");
                tpo.SendSMS(qmMessage);
                Configure.LogData(qmMessage, null);
            }
            catch (Exception ex) 
            {
                Configure.LogData(message, ex);
                throw;
            }
        }

    }
}