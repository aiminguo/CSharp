using Tpo.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

namespace TestSMS
{
    /// <summary>
    ///This is a test class for TPONotification and is intended
    ///to contain all TPOWebServiceTestSMS Unit Tests
    ///</summary>
    [TestClass()]
    public class TPONotificationTest
    {
        /// <summary>
        ///A test for GetGateWayProviders
        ///</summary>
        [TestMethod()]
        public void GetGateWayProvidersTest()
        {
            TPONotification target = new TPONotification(); 
            string token = string.Empty; 
            GateWayProvider[] expected = null; 
            GateWayProvider[] actual;
            actual = target.GetGateWayProviders(token);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for SendAlert
        ///</summary>
        [TestMethod()]
        public void SendAlertTestInvalidToken()
        {
            TPONotification target = new TPONotification(); 
            string token = string.Empty; 
            SMSMessage smsMessage = null; 
            Response actual = target.SendAlert(token, smsMessage);
            Assert.IsTrue( actual.StatusCode < 200);
        }

        [TestMethod()]
        public void SendAlertTestInvalidRequest()
        {
            TPONotification target = new TPONotification();
            string token = "0A2462FD-EC5F-49A9-AC52-7369660A1241";
            SMSMessage smsMessage = null;
            Response actual = target.SendAlert(token, smsMessage);
            Assert.IsTrue(actual.StatusCode < 200);
        }

        [TestMethod()]
        public void SendAlertTestInvalidEmail()
        {
            TPONotification target = new TPONotification();
            string token = "0A2462FD-EC5F-49A9-AC52-7369660A1241";
            SMSMessage smsMessage = new SMSMessage {
                ToAddress ="wssss@sss.com\\"
            };
            Response actual = target.SendAlert(token, smsMessage);
            Assert.IsTrue(actual.StatusCode < 200);
        }
    }
}
