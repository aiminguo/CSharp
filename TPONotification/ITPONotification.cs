using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace Tpo.Notification
{
    [ServiceContract]
    public interface ITPONotification
    {

        [OperationContract]
        [WebGet(UriTemplate = "/getGateWayProviders/{token}",
            ResponseFormat = WebMessageFormat.Json)]
        GateWayProvider[] GetGateWayProviders(string token);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "/sendAlert", 
            BodyStyle=WebMessageBodyStyle.WrappedRequest,
            RequestFormat = WebMessageFormat.Json,
            ResponseFormat = WebMessageFormat.Json)]
        Response SendAlert(string token, SMSMessage sms);

    }

#region DataContract
    public class GateWayProvider
    {
        /// <summary>
        /// Provider Id.
        /// </summary>
        [DataMember(Name = "providerId", Order = 1)]
        public string ProviderId { get; set; }

        /// <summary>
        /// SMS Address.
        /// </summary>
        [DataMember(Name = "smsAddress", Order = 2)]
        public string SmsAddress { get; set; }

        /// <summary>
        /// Provider Name.
        /// </summary>
        [DataMember(Name = "providerName", Order = 3)]
        public string ProviderName { get; set; }
    }
    [DataContract(Name = "SMSMessage")]
    public partial class SMSMessage
    {
        [DataMember(Name = "fromAddress")]
        public String FromAddress { get; set; }

        [DataMember(Name = "fromName")]
        public String FromName { get; set; }

        [DataMember(Name = "toAddress")]
        public String ToAddress { get; set; }

        [DataMember(Name = "subject")]
        public String Subject { get; set; }

        [DataMember(Name = "textBody")]
        public String TextBody { get; set; }

        [DataMember(Name = "scheduledDateTime")]
        public DateTime ScheduledDateTime { get; set; }

        [DataMember(Name = "client")]
        public String Client { get; set; }
    }

    [DataContract(Name = "Response")]
    public class Response
    {
        [DataMember(Name = "statusCode")]
        public int StatusCode;
        [DataMember(Name = "message")]
        public String Message;
    }

    //[DataContract ]
    //public class Auth
    //{
    //    [DataMember(Name = "version")]
    //    public String Version { get; set; }
    //    [DataMember(Name = "clientId")]
    //    public Guid ClientId { get; set; }
    //    [DataMember(Name = "tpoUserName")]
    //    public string TpoUserName { get; set; }
    //}

    #endregion}

}
