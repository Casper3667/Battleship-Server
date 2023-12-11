using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Diagnostics;
using System.Text;
using System;
using GameServer.Client_Facing.Messages;
using GameServer.Client_Facing;

namespace GameServer.Chat_ServiceFacing
{
    internal static class Consumer
    {
        //public static string URL = "192.168.87.176";
        public static Queue<string> Messages = new Queue<string>();
        public static Mutex MessagesLock = new Mutex();


        public delegate void HandleMessageDelegate(string message);

        #region AnyChat
        public static void ListenToChat(ChatServiceInterface owner, string URL, Producer.ChatGroup chatGroup, List<string> routingKeys,HandleMessageDelegate handleMessage)
        {
            Producer.ChatGroupStruct chat = Producer.ChatGroups[chatGroup];
            Player player = owner.player;

            var factory = new ConnectionFactory() { HostName = URL };
            factory.UserName = "Client";
            factory.Password = "guest";
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {

                channel.ExchangeDeclare(chat.Name, chat.exchangeType);
                var queueName = channel.QueueDeclare().QueueName;

                foreach (var rkey in routingKeys)
                {

                    //Debug.WriteLine($"Subscribing to Routing Key: {rkey}");
                    channel.QueueBind(queue: queueName,
                    exchange: chat.Name,
                    routingKey: rkey);
                }
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    //Console.WriteLine("[X] Recieved " + message);
                    //MessagesLock.WaitOne();
                    //Messages.Enqueue(message);
                    //MessagesLock.ReleaseMutex();

                    handleMessage(message);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                };

                while (player.client.Connected)
                {
                    channel.BasicConsume(queue: queueName,
                    autoAck: false,
                    consumer: consumer);
                }

            }
        }

        #endregion
        #region Public Chat
        //private static bool PublicChatRunning = false;
        // private static Thread PublicChatConsumerThread;

        //public static void StartPublicChatConsumerThread()
        //{
        //    PublicChatConsumerThread = new Thread(ListenToPublicChat);
        //    PublicChatConsumerThread.IsBackground = true;
        //    PublicChatRunning = true;
        //    PublicChatConsumerThread.Start();

        //}
        //public static void StopPublicChatConsumerThread()
        //{
        //    PublicChatRunning = false;

        //}

        //public static void ListenToPublicChat()
        //{
        //    var factory = new ConnectionFactory() { HostName = URL };
        //    factory.UserName = "SessionService";
        //    factory.Password = "guest";
        //    using (var connection = factory.CreateConnection())
        //    using (var channel = connection.CreateModel())
        //    {


        //        channel.ExchangeDeclare("Public Chat", ExchangeType.Fanout);
        //        var queueName = channel.QueueDeclare().QueueName;
        //        channel.QueueBind(queue: queueName,
        //            exchange: "Public Chat",
        //            routingKey: "");
        //        var consumer = new EventingBasicConsumer(channel);
        //        consumer.Received += (model, ea) =>
        //        {
        //            var body = ea.Body.ToArray();
        //            var message = Encoding.UTF8.GetString(body);
        //            //Console.WriteLine("[X] Recieved " + message);
        //            MessagesLock.WaitOne();
        //            Messages.Enqueue(message);
        //            MessagesLock.ReleaseMutex();
        //            //int dots = message.Split('.').Length - 1;

        //            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        //        };

        //        while (PublicChatRunning)
        //        {
        //            channel.BasicConsume(queue: queueName,
        //            autoAck: false,
        //            consumer: consumer);
        //        }



        //    }
        //}
        #endregion
        #region Group Chat
        //private static bool GroupChatRunning = false;
        //// private static Thread GroupChatConsumerThread;

        ////private static Dictionary<string, bool> GroupChatRunnings=new Dictionary<string, bool> ();

        //public static void StartGroupChatConsumerThread(List<string> routingKeys)
        //{
        //    if (GroupChatRunning == false/*GroupChatRunnings.ContainsKey(groupname)==false*/)
        //    {
        //        var groupChatConsumerThread = new Thread(() => ListenToGroupChat(routingKeys));
        //        groupChatConsumerThread.IsBackground = true;
        //        //GroupChatRunnings.Add(groupname, true);
        //        GroupChatRunning = true;
        //        groupChatConsumerThread.Start();
        //    }
        //    else
        //    {

        //        Debug.WriteLine("GroupChat is Already Running");
        //    }

        //}
        //public static void StopGroupChatConsumerThread(/*string groupname*/)
        //{
        //    GroupChatRunning = false;
        //    //if(GroupChatRunnings.ContainsKey(groupname))
        //    //    {
        //    //        GroupChatRunnings[groupname] = false;
        //    //    }
        //    //    else
        //    //    {
        //    //        Debug.WriteLine("Could not Find A Active Group with the name " + groupname);
        //    //    }

        //}
        //public static void ListenToGroupChat(List<string> routingKeys)
        //{
        //    var factory = new ConnectionFactory() { HostName = URL };
        //    factory.UserName = "Client";
        //    factory.Password = "guest";
        //    using (var connection = factory.CreateConnection())
        //    using (var channel = connection.CreateModel())
        //    {
        //        channel.ExchangeDeclare("Group Chat", ExchangeType.Direct);
        //        var queueName = channel.QueueDeclare().QueueName;

        //        foreach (var rkey in routingKeys)
        //        {
        //            Debug.WriteLine($"Subscribing to Routing Key: {rkey}");
        //            channel.QueueBind(queue: queueName,
        //            exchange: "Group Chat",
        //            routingKey: rkey);
        //        }
        //        var consumer = new EventingBasicConsumer(channel);
        //        consumer.Received += (model, ea) =>
        //        {
        //            var body = ea.Body.ToArray();
        //            var message = Encoding.UTF8.GetString(body);
        //            //Console.WriteLine("[X] Recieved " + message);
        //            MessagesLock.WaitOne();
        //            Messages.Enqueue(message);
        //            MessagesLock.ReleaseMutex();

        //            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        //        };

        //        while (GroupChatRunning)
        //        {
        //            channel.BasicConsume(queue: queueName,
        //            autoAck: false,
        //            consumer: consumer);
        //        }

        //    }
        //}
        #endregion




    }
}
