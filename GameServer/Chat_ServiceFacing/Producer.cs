using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;

namespace GameServer.Chat_ServiceFacing
{
    internal static class Producer
    {
        //public static string URL = "192.168.87.176";

        public enum ChatGroup
        {
            Private,
            Group,
            Public,
        }
        public struct ChatGroupStruct
        {
            public string Name;
            public string exchangeType;
        }
        static ChatGroupStruct privatechat = new() { Name = "PrivateChat", exchangeType = ExchangeType.Direct };
        static ChatGroupStruct groupchat = new() { Name = "GroupChat", exchangeType = ExchangeType.Direct };
        static ChatGroupStruct publichat = new() { Name = "PublicChat", exchangeType = ExchangeType.Fanout };

        public static Dictionary<ChatGroup, ChatGroupStruct> ChatGroups = new()
        {
            {ChatGroup.Private,privatechat},
            {ChatGroup.Group,groupchat },
            {ChatGroup.Public,publichat}
        };
            


        public static void ConnectToChatServiceAndSend(string URL,ChatGroup Chatgroup,string? routingKey, string message)
        {
            var factory = new ConnectionFactory() { HostName = URL };
            factory.UserName = "Client";
            factory.Password = "guest";
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var chat = ChatGroups[Chatgroup];
                channel.ExchangeDeclare(chat.Name, chat.exchangeType);
                SendChatMessageViaExchange(message, channel,chat.Name, routingKey);
            }
        }

        //public static void ConnectToGroupChatAndSend(string routingKey, string message)
        //{
        //    var factory = new ConnectionFactory() { HostName = URL };
        //    factory.UserName = "Client";
        //    factory.Password = "guest";
        //    using (var connection = factory.CreateConnection())
        //    using (var channel = connection.CreateModel())
        //    {
        //        channel.ExchangeDeclare("Group Chat", ExchangeType.Direct);
        //        SendGroupMessageViaExchange(message, channel, routingKey);
        //    }
        //}

        private static void SendChatMessageViaExchange(string message, IModel channel,string Name, string? routingKey)
        {
            if (routingKey == null) routingKey = "";
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: Name,
                routingKey: routingKey,
                basicProperties: null,
                body: body);

            Console.WriteLine(
                $"[PRODUCER]: " +
                $"\nSent Message    : {message}" +
                $"\nTo Exchange     : {Name}" +
                $"\nUsing RoutingKey: {routingKey}");

            //Console.WriteLine("[X] Sent {0}", message);
            //Debug.WriteLine("[X] Sent {0}", message);
        }
    }
    
}
