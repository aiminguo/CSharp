using System;
using System.Collections;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Tpo.Infrastructure.Messaging;

namespace Tpo.Notification
{
    public class SMS
    {
        private SmtpClient _ss;
        private DbmqMessageController _dbMQ;

        public SMS() 
        {
            _dbMQ = new DbmqMessageController(Configure.MessageQueueDB, Configure.DBMQFileLocation);
            _ss = new SmtpClient(Configure.SMTPServer)
            {
                Timeout = 5000
            };
        }

        public void AddDBMQ(string message, DateTime? scheduledDatetime = null, int priority = 10)
        {
            if (null == _dbMQ)
                _dbMQ = new DbmqMessageController(Configure.MessageQueueDB, Configure.DBMQFileLocation);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<qm caller='sms'></qm>");

            //Create a CData section.
            XmlCDataSection cData = doc.CreateCDataSection(message);    

            //Add the new node to the document.
            XmlElement root = doc.DocumentElement;
            root.AppendChild(cData); 

            _dbMQ.AddMessage(Configure.QueueId,
                doc.InnerXml,
                Guid.NewGuid(),
                null,
                scheduledDatetime,
                priority);
        }

        public bool IsInvalidEmail(string emailAddress)
        {
            Regex emailRegex = new Regex(
                @"^[A-Z0-9.!$&*-=^`|~#%'+/?_{}]+@" +
                //@"^(?("")(""[^""]+?""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                @"\w[-._\w]*\w\.\w{2,3}$",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

            var matches = emailRegex.Matches(emailAddress);

            return matches.Count == 0;
        }

        /// <summary>
        /// Sends an email using the parameters you provide
        /// </summary>
        /// <param name="to">The receipient of the message.</param>
        /// <param name="from">The sender of the message.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body of the message to be sent.</param>
        public void Send(string fromEmail, string fromDisplayName, string toEmail,
            string subject, string body, bool isHTML)
        {
            if (null == _ss)
            {
                //smtpServer = "smtp.tpolab.com";
                _ss = new SmtpClient(Configure.SMTPServer)
                {
                    Timeout = 5000
                };
            }

            MailAddress from = new MailAddress(fromEmail, fromDisplayName);
            //this is the way to support multiple toEmail addresses
            using (MailMessage message = new MailMessage(string.Format("{0} <{1}>",fromDisplayName, fromEmail), toEmail))
            {
                // Set body type
                if (isHTML)
                {
                    message.IsBodyHtml = true;
                    message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;
                }
                else
                {
                    message.IsBodyHtml = false;
                    message.BodyEncoding = System.Text.Encoding.ASCII;
                    message.DeliveryNotificationOptions = DeliveryNotificationOptions.None;
                }
                // Assign message body and subject
                message.Body = body;
                message.Subject = subject;
                _ss.Send(message);
                //System.Threading.Thread.Sleep(15);
            }
        }
    }
}
