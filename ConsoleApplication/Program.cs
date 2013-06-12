using System;
using System.Web;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using SMSConsoleApplication.ServiceReference1;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            TPONotificationClient client = new TPONotificationClient("sms");
            string token = "0A2462FD-EC5F-49A9-AC52-7369660A1241"; //Guid.NewGuid().ToString("D");
            //[System.ServiceModel.Web.WebGet(UriTemplate = "/getGateWayProviders/{token}")]
            //http://localhost:38570/Notification.svc/getGateWayProviders/0A2462FD-EC5F-49A9-AC52-7369660A1241
            //http://tpodev10web01.tpolab.com:38570/Notification.svc/getGateWayProviders/0A2462FD-EC5F-49A9-AC52-7369660A1241
            //var gps = client.GetGateWayProviders(token);
            //foreach (GateWayProvider gp in gps)
            //{
            //    Console.WriteLine("{0} - {1} - {2}", gp.ProviderName, gp.SmsAddress, gp.ProviderId);
            //}

            SMSMessage sms = new SMSMessage()
            {
                subject = "LPS Lead local test",
                fromAddress = "la@tpolab.com",
                fromName = "TPOLab.com",
                toAddress = "aimin@topproducersystems.com", //"aimin@topproducersystems.com",
                //toAddress = "aimin.guo@move.com",
                //scheduledDateTime =  DateTime.UtcNow,
                client = "Email",
                textBody = String.Format("{0}\r{1}\r{2}\r{3}",
                    "John Smith",
                    "778-123-5678",
                    "<b>loadtest@tpolab.com</b>",
                    "Buyer")

                /* TPO lead alert
                    John Smith
                    778-123-5678
                    tm@topproducer.com
                    Welcome Wagon <- Lead Source
                    Buyer
                 */
            };

            /** BodyStyle lined have to be added to Service References\TPOWebService\Reference.cs file
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/ITPOWebService/SendAlert", ReplyAction="http://tempuri.org/ITPOWebService/SendAlertResponse")]
        [System.ServiceModel.Web.WebInvoke(BodyStyle = System.ServiceModel.Web.WebMessageBodyStyle.WrappedRequest)]
        ConsoleApplication1.TPOWebService.Response SendAlert(string token, ConsoleApplication1.TPOWebService.SMSMessage sms);            
<SendAlert xmlns="http://tempuri.org/"><token>0A2462FD-EC5F-49A9-AC52-7369660A1241</token>
<sms xmlns:a="http://schemas.datacontract.org/2004/07/Tpo.Notification" xmlns:i="http://www.w3.org/2001/XMLSchema-instance">
<a:fromAddress>la@Realtor.com</a:fromAddress><a:fromName>Realtor.com</a:fromName>
<a:scheduledDateTime>2012-10-03T22:23:57.2536607Z</a:scheduledDateTime>
<a:subject>LPS Lead SMS Message</a:subject>
<a:textBody>John Smith&#xD;778-123-5678&#xD;tm@topproducer.com&#xD;Buyer</a:textBody>
<a:toAddress>aimin@topproducersystems.com</a:toAddress>
</sms></SendAlert>             
            */
            SendAlertRequest req = new SendAlertRequest(token, sms);
            var r = client.SendAlert(req);
            //Console.WriteLine("{0, 3} - {1}", r.statusCode, r.message);
            Console.ReadLine();
        }

    }
}
