using System;
using System.IO;
using System.Text;
using System.Data;
using System.Linq;
using System.Data.SqlClient;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text.RegularExpressions;


namespace Tpo.Notification
{
    public struct StatusCode
    {
        public const int Sent = 200;
        public const int Queued = 201;
        public const int InvalidEmail = 1;
        public const int InvalidToken = 2;
        public const int InvalidRequest = 3;
    }

    //[AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    //[ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    [Serializable]
    public class TPONotification : ITPONotification
    {
        private static readonly DataContractJsonSerializer _jsonSerializer = new DataContractJsonSerializer(typeof(SMSMessage));

        public GateWayProvider[] GetGateWayProviders(string token)
        {
            if (IsInvalidToken(token))
            {
                return null;
            }
            return LoadSMSProviders(Configure.SMSProviderDB);
        }

        public Response SendAlert(string token, SMSMessage smsMessage)
        {
            try
            {
                if (IsInvalidToken(token))
                {
                    return GetResponse(StatusCode.InvalidToken);
                }
                if (null == smsMessage)
                {
                    return GetResponse(StatusCode.InvalidRequest);
                }

                if (string.IsNullOrEmpty(smsMessage.Client)) 
                {
                    smsMessage.Client = smsMessage.FromAddress;
                }

                if (Configure.LogCaller)
                {
                    LogClientInfo(token, smsMessage.Client);
                }

                DateTime scheduledDateTime = smsMessage.ScheduledDateTime;
                DateTime localDateTime = DateTime.Now;

                //convert the UTC time to server time
                if (DateTime.MinValue == scheduledDateTime)
                    scheduledDateTime = localDateTime;
                else
                    scheduledDateTime = scheduledDateTime.ToLocalTime();

                SMS eh = Configure._sms;
                //in case of multiple toEmail addresses test@fake.com ; ,  ;test'@fake.com
                string emailList = smsMessage.ToAddress.Replace(" ", "").Replace(";", ",");
                var el = new List<string>(emailList.Split(','));
                el.RemoveAll(e => e.Length == 0);
                if (el.Any(item => eh.IsInvalidEmail(item)))
                {
                    return GetResponse(StatusCode.InvalidEmail);
                }

                //make it a standard toEmail list
                smsMessage.ToAddress = string.Join(",", el.ToArray());
                string qmMessage = SerializeSMSMessage(smsMessage);
                //smsMessage = DeserializeSMSMessage(s);

                log4net.GlobalContext.Properties["time"] = localDateTime;
                log4net.GlobalContext.Properties["email"] = smsMessage.ToAddress;

                if (scheduledDateTime <= localDateTime && !Configure.AsynMode)
                {
                    SendSMS(smsMessage);
                    Configure.LogData(ReplaceDate(qmMessage, scheduledDateTime), null);
                    return GetResponse(StatusCode.Sent);
                }
                else
                {
                    eh.AddDBMQ(qmMessage, scheduledDateTime);
                    Configure.LogData(ReplaceDate(qmMessage,scheduledDateTime), null);
                    return GetResponse(StatusCode.Queued);
                }
            }
            catch (Exception ex)
            {
                Configure.LogData("SendAlert call", ex);
                throw;
            }
        }

        #region helpfunction
        internal string ReplaceDate(string qmMessage, DateTime dt) {
            // @"\/Date(.+)\/"
            return Regex.Replace(qmMessage, @"\\/Date(.+)\/", dt.ToString("yyyy-MM-dd HH:mm:ss tt"));
        }
        internal void SendSMS(string qmMessage)
        {
            SMSMessage smsMessage = DeserializeSMSMessage(qmMessage);
            log4net.GlobalContext.Properties["email"] = smsMessage.ToAddress;
            SendSMS(smsMessage);
        }
        internal void SendSMS(SMSMessage smsMessage)
        {
            SMS sms = Configure._sms;
            bool isHTML = smsMessage.Client.Equals("Email", StringComparison.CurrentCultureIgnoreCase) ? true : false;
            sms.Send(smsMessage.FromAddress, smsMessage.FromName, smsMessage.ToAddress,
                smsMessage.Subject, smsMessage.TextBody, isHTML);
        }

        private bool IsInvalidToken(string token) 
        {
            if (string.IsNullOrEmpty(token))
                return true;
            return !Regex.Match(Configure.Tokens, token, RegexOptions.IgnoreCase).Success;
        }

        private Response GetResponse(int statusCode)
        {
            string message = "";
            switch (statusCode)
            {
                case StatusCode.Sent:
                    message = "SMS was sent.";
                    break;
                case StatusCode.Queued:
                    message = "SMS was queued.";
                    break;
                case StatusCode.InvalidEmail:
                    message = "ToAddress email is invalid.";
                    break;
                case StatusCode.InvalidToken:
                    message = "The token is invalid.";
                    break;
                case StatusCode.InvalidRequest:
                    message = "The SMS Message is invalid.";
                    break;
            }
            return new Response
            {
                StatusCode = statusCode,
                Message = message
            };
        }

        private void LogClientInfo(string token, string client) 
        {
            using (var connection = new SqlConnection(Configure.LogDB))
            {
                connection.Open();
                var command = new SqlCommand("webrequest_count_set", connection);
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add(new SqlParameter("client_id", new Guid(token)));
                command.Parameters.Add(new SqlParameter("caller", client));

                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        private GateWayProvider[] LoadSMSProviders(string connectionString)
        {
            List<GateWayProvider> results = new List<GateWayProvider>();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var command = new SqlCommand("witp_get_sms_providers", connection)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GateWayProvider gp = new GateWayProvider 
                        {
                            ProviderId = reader[0].ToString(),
                            ProviderName = reader[6].ToString(),
                            SmsAddress = reader[2].ToString()
                        };
                        results.Add(gp);
                    }
                }
            }
            return results.ToArray();
        }

        private static string SerializeSMSMessage(SMSMessage results)
        {
            using (var stream = new MemoryStream())
            {
                _jsonSerializer.WriteObject(stream, results);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private SMSMessage DeserializeSMSMessage(string resultsString)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(resultsString)))
            {
                return (SMSMessage)_jsonSerializer.ReadObject(stream);
            }
        }
        #endregion
    }
}
