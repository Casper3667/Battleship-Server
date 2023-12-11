using GameServer;
using GameServer.Client_Facing;
using GameServer.Client_Facing.Messages;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameTest
{
    internal class ClientMessageHandler_Tests
    {
        ClientMessageHandler handler;

        ShotMessage Default_Valid_ShotMessage = new ShotMessage() { X = 1, Y = 1, ValidShotMessage = true};
        RawChatMessageFromClient Default_Valid_RawChatMessageFromClient = new RawChatMessageFromClient() { From = "Sofie",To=ChatType.Private, Message = "Hello World", ValidRawChatMessageFromClient = true };

        [SetUp]
        public void Setup()
        {
            Testing.IsTesting = true;
            GameServer.GameServer server = new(IPAddress.Any, 000, 2);
            Player player = new Player("player 1", new System.Net.Sockets.TcpClient(), server, new JWT("player 1", DateTime.Now, DateTime.Now.AddDays(2)));

            handler = player.ClientMessageHandler;
            
        }

        [Test]
        public void Test_MessageHandler_Setup()
        {
            Assert.Pass();
        }

        #region - LatestShotMessage TESTS
        [Test]
        public void Test_MessageHandler_LatestShotMessages_Set() {

            ShotMessage input = new() { X=1,Y=1};
            handler.TEST_Set_LatestMessage(input);
            
            Assert.Pass();
        }
        [Test]
        public void Test_MessageHandler_LatestShotMessages_Get_Null()
        {
            ShotMessage? expect = null;
            ShotMessage? result = handler.LatestShotMessage;


            Assert.That(result,Is.EqualTo(expect));
        }
        [Test]
        public void Test_MessageHandler_LatestShotMessages_Get_ShotMessage()
        {
            ShotMessage? input = new() { X = 1, Y = 1 };
            ShotMessage? expect1 = input;
            ShotMessage? expect2 = null;
            // To Get a Shot Message Out We first need to make sure that there is anything in Latest Shot message
            handler.TEST_Set_LatestMessage(input);

            
            ShotMessage? result1 = handler.LatestShotMessage;
            ShotMessage? result2 = handler.LatestShotMessage;


            Assert.Multiple(() =>
            {
                Assert.That(result1, Is.EqualTo(expect1),"Test Where Latest Shot Message Should Have a Message Stored");
                Assert.That(result2, Is.EqualTo(expect2), "Test Where there should be no Chat Message since the last Test Took It");
            });
        }
        #endregion
        #region - DeserializeMessage TESTS
        [Test]
        public void Test_MessageHandler_DeserializeMessage_ShotMessage_Valid()
        {
            ShotMessage input = Default_Valid_ShotMessage;
            string jsonstring = JsonSerializer.Serialize(input);
            ShotMessage expect = input;
            IClientMessage? result =handler.DeserializeMessage<ShotMessage>(jsonstring);

            string expectstring = "{\"X\":1,\"Y\":1,\"ValidShotMessage\":true}";

            Assert.Multiple(() =>
            {
                Assert.That(
                    (((ShotMessage)result).X==input.X)==
                    (((ShotMessage)result).Y==input.Y) ==
                    (((ShotMessage)result).ValidShotMessage==input.ValidShotMessage)
                    ,Is.True, "Main Test: Is the result the same as the original input");
                Assert.That(((ShotMessage)result).ValidShotMessage,Is.True, "Side Test: Is Result Valid");
                Assert.That(jsonstring,Is.EqualTo(expectstring), "Side Test: Is The json String what you would expect");
            });
        }
        [Test]
        public void Test_MessageHandler_DeserializeMessage_ShotMessage_Invalid()
        {
            /// Gives Wrong Input So that it should result in an invalid Shot Message
            RawChatMessageFromClient input = Default_Valid_RawChatMessageFromClient; 
            string jsonstring = JsonSerializer.Serialize(input);
            bool expectValid = false;
            IClientMessage? result = handler.DeserializeMessage<ShotMessage>(jsonstring);

            string expectstring = "{\"From\":\"Sofie\",\"To\":0,\"Message\":\"Hello World\",\"ValidRawChatMessageFromClient\":true}";

            Assert.Multiple(() =>
            {
                Assert.That(((ShotMessage)result).ValidShotMessage, Is.EqualTo(expectValid), "Main Test: Tests to see if Result Invalid");
                Assert.That(result, Is.TypeOf<ShotMessage>(), "Side Test: Is the result a Shot Message");
                Assert.That(jsonstring, Is.EqualTo(expectstring), "Side Test: Is The json String what you would expect");
            });
        }
        [Test]
        public void Test_MessageHandler_DeserializeMessage_RawChatMessageFromClient_Valid_Private()
        {
            RawChatMessageFromClient input = Default_Valid_RawChatMessageFromClient;
            string jsonstring = JsonSerializer.Serialize(input);
            RawChatMessageFromClient expect = input;
            IClientMessage? result = handler.DeserializeMessage<RawChatMessageFromClient>(jsonstring);
            string expectstring = "{\"From\":\"Sofie\",\"To\":0,\"Message\":\"Hello World\",\"ValidRawChatMessageFromClient\":true}";
            Assert.Multiple(() =>
            {
            Assert.That(
                ((RawChatMessageFromClient)result).From == input.From ==
                (((RawChatMessageFromClient)result).Message == input.Message) ==
                (((RawChatMessageFromClient)result).ValidRawChatMessageFromClient== input.ValidRawChatMessageFromClient)
                , Is.True, "Main Test: Is the result the same as the original input");;
                Assert.That(((RawChatMessageFromClient)result).ValidRawChatMessageFromClient, Is.True, "Side Test: Is Result Valid");
                Assert.That(jsonstring, Is.EqualTo(expectstring), "Side Test: Is The json String what you would expect");
                Assert.That(((RawChatMessageFromClient)result).To, Is.EqualTo(input.To),"Checks That Message Type is Correct");
            });
        }
        [Test]
        public void Test_MessageHandler_DeserializeMessage_RawChatMessageFromClient_Valid_Group()
        {
            RawChatMessageFromClient input = new RawChatMessageFromClient() { From = "Sofie", To = ChatType.Group, Message = "Hello World", ValidRawChatMessageFromClient = true };
            string jsonstring = JsonSerializer.Serialize(input);
            RawChatMessageFromClient expect = input;
            IClientMessage? result = handler.DeserializeMessage<RawChatMessageFromClient>(jsonstring);
            string expectstring = "{\"From\":\"Sofie\",\"To\":1,\"Message\":\"Hello World\",\"ValidRawChatMessageFromClient\":true}";
            Assert.Multiple(() =>
            {
                Assert.That(
                    ((RawChatMessageFromClient)result).From == input.From ==
                    (((RawChatMessageFromClient)result).Message == input.Message) ==
                    (((RawChatMessageFromClient)result).ValidRawChatMessageFromClient == input.ValidRawChatMessageFromClient)
                    , Is.True, "Main Test: Is the result the same as the original input"); ;
                Assert.That(((RawChatMessageFromClient)result).ValidRawChatMessageFromClient, Is.True, "Side Test: Is Result Valid");
                Assert.That(jsonstring, Is.EqualTo(expectstring), "Side Test: Is The json String what you would expect");
                Assert.That(((RawChatMessageFromClient)result).To, Is.EqualTo(input.To), "Checks That Message Type is Correct");
            });
        }
        [Test]
        public void Test_MessageHandler_DeserializeMessage_RawChatMessageFromClient_Invalid()
        {
            /// Gives Wrong Input So that it should result in an invalid RawChatMessageFromClient
            ShotMessage input = Default_Valid_ShotMessage;
            string jsonstring = JsonSerializer.Serialize(input);
            bool expectValid = false;
            IClientMessage? result = handler.DeserializeMessage<RawChatMessageFromClient>(jsonstring);
            string expectstring = "{\"X\":1,\"Y\":1,\"ValidShotMessage\":true}";
            Assert.Multiple(() =>
            {
                Assert.That(((RawChatMessageFromClient)result).ValidRawChatMessageFromClient, Is.EqualTo(expectValid), "Main Test: Tests to see if Result Invalid");
                Assert.That(result, Is.TypeOf<RawChatMessageFromClient>(), "Side Test: Is the result a RawChatMessageFromClient");
                Assert.That(jsonstring, Is.EqualTo(expectstring), "Side Test: Is The json String what you would expect");
            });
        }
        [Test]
        public void Test_MessageHandler_DeserializeMessage_Null()
        {
            // ShotMessage input = Default_Valid_ShotMessage;
            string input ="Hello World";
            ShotMessage? expect = null;
            IClientMessage? result = handler.DeserializeMessage<ShotMessage>(input);

            //string expectstring = "{\"X\":1,\"Y\":1,\"ValidShotMessage\":true}";

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expect), "Main Test: Is the result Null");
                //Assert.Warn("There was a Deserialization failed Warning");
            });
            
            
            
        }
        #endregion
        #region - SortClientMessages TESTS
        [Test]
        public void Test_MessageHandler_SortClientMessage_ShotMessage()
        {
            ShotMessage input = Default_Valid_ShotMessage;
            string jsonstring = JsonSerializer.Serialize(input);
            string expect = "ShotMessage";
            string expectJsonString = "{\"X\":1,\"Y\":1,\"ValidShotMessage\":true}";

           string result= handler.SortClientMessage(jsonstring);

            

           Assert.Multiple(() =>
           {
               Assert.That(result,Is.EqualTo(expect), "Main Test: Test that input was Sorted Correctly");
               Assert.That(jsonstring,Is.EqualTo(expectJsonString), "Side Test: Test that Input was Serialized Correctly");
           });
        }
        [Test]
        public void Test_MessageHandler_SortClientMessage_RawChatMessageFromClient()
        {
            RawChatMessageFromClient input = Default_Valid_RawChatMessageFromClient;
            string jsonstring = JsonSerializer.Serialize(input);
            string expect = "RawChatMessageFromClient";        
            string expectJsonString = "{\"From\":\"Sofie\",\"To\":0,\"Message\":\"Hello World\",\"ValidRawChatMessageFromClient\":true}";
            
            string result = handler.SortClientMessage(jsonstring);

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.EqualTo(expect), "Main Test: Test that input was Sorted Correctly");
                Assert.That(jsonstring, Is.EqualTo(expectJsonString), "Side Test: Test that Input was Serialized Correctly");
            });
        }
        [Test]
        public void Test_MessageHandler_SortClientMessage_InvalidMessage()
        {
            string input = "Not a Json String";
            ShotMessage shotMessage = new ShotMessage(); // This is an Invalid Shot Message
            RawChatMessageFromClient chatMessage= new RawChatMessageFromClient(); // This Is An Invalid RawChatMessageFromClient
            string jsonShotMessage= JsonSerializer.Serialize(shotMessage);
            string jsonChatMessage= JsonSerializer.Serialize(chatMessage);

            string expect = "InvalidMessage";
            string expectJsonShotMessage = "{\"X\":0,\"Y\":0,\"ValidShotMessage\":false}";
            string expectJsonChatMessage = "{\"From\":\"\",\"To\":0,\"Message\":\"\",\"ValidRawChatMessageFromClient\":false}";

            Assert.Multiple(() =>
            {
                Assert.That(handler.SortClientMessage(input), Is.EqualTo(expect), "Test with strign that isnt a json string");
                Assert.That(handler.SortClientMessage(jsonShotMessage), Is.EqualTo(expect), "Test with Invalid Shot Message");
                Assert.That(jsonShotMessage, Is.EqualTo(expectJsonShotMessage), "Checks that Json string is correct for Invalid Shot Message");
                Assert.That(handler.SortClientMessage(jsonChatMessage), Is.EqualTo(expect), "Test with Invalid Chat Message");
                Assert.That(jsonChatMessage, Is.EqualTo(expectJsonChatMessage), "Checks that Json string is correct for Invalid Chat Message");
            });

            
        }
        #endregion

    }
}
