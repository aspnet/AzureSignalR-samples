using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.SignalR.Samples.ReliableChatRoom.Entities;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ReliableChatRoomUnitTest.Entities
{
    [TestClass]
    public class SessionUnitTest
    {
        private readonly string _username = "john";
        private readonly string _connectionId = "ABC";
        private readonly string _deviceUuid = "DEF";

        [TestMethod]
        public void Test_Expire()
        {
            // Set up
            Session session = new Session(_username, _connectionId, _deviceUuid);

            // Operation
            session.Expire();

            // Assertion
            Assert.AreEqual(session.SessionType, SessionTypeEnum.Expired);
        }
    }
}
