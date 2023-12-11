using GameServer.Client_Facing;
using GameServer.Client_Facing.Messages;
using GameServer.Settings;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace GameServer.Chat_ServiceFacing
{
    public class ChatServiceInterface
    {
        string? IP;
        int? Port;

        public Player player { get; private set; }
        NetworkStream stream;

        bool startedCorrectly;

        public ChatServiceInterface(Player _player,NetworkStream _stream)
        {
            player = _player;
            stream = _stream;
            try
            {
                
                Settings.Settings.LoadSettings();
                IP = Settings.Settings.ServerSettings.ChatServiceIP;
                Port = Settings.Settings.ServerSettings.ChatServicePort;
                startedCorrectly= true;
            }
            catch (Exception e)
            {
                Print("Failed To Make ChatService: " + e);
                //throw;
            }   
            
        }
        public void StartListeningToChatService()
        {
            
            if(startedCorrectly)
            {
                try
                {
                    SetupPrivateChatAndGroupChatListners();
                    SetupPublicChatListner();

                }
                catch (Exception e)
                {
                    Print("Failed in Setting up Listners: " + e);
                }
            }
            else
            {
                Print("Cant Start Listening since It Didnt Start Correctly");
            }
        }

        #region ------------------Listening (Consumer)----------------------
       private void SendMessage(RawChatMessage chatmessage)
        {
            string message=JsonSerializer.Serialize(chatmessage);
            Print("Sending Message To Client: " + message);

            player.gameServer.SendMessageLine(message, stream);
        }
        
        #region Private & Group
        Thread PrivateChatThread;
        Thread GroupChatThread;
        private bool SetupPrivateChatAndGroupChatListners()
        {
            try
            {
                if (IP != null)
                {
                    GameServer server = player.gameServer;
                    // string serverkey = server.address + ":" + server.port;
                    string serverkey = server.server.LocalEndpoint+"";
                    List<string> pkeys = new List<string>() { player.Username, serverkey };
                    Consumer.HandleMessageDelegate handler = HandleClientMessageFromService;
                    PrivateChatThread = new(() => Consumer.ListenToChat(this, IP, Producer.ChatGroup.Private, pkeys, handler)) { IsBackground = true };
                    
                    List<string> gkeys = new List<string>() { server.LobbyID };
                    GroupChatThread = new(() => Consumer.ListenToChat(this, IP, Producer.ChatGroup.Group, gkeys, handler)) { IsBackground = true };

                    PrivateChatThread.Start();
                    PrintThread(Producer.ChatGroup.Private, pkeys);
                    GroupChatThread.Start();
                    PrintThread(Producer.ChatGroup.Group, gkeys);

                }
                return true;
            }
            catch (Exception e)
            {
                Print("Failed when setting up Private chat and Group chat Listners: " + e);
                return false;
            }
        }
        
        private void  HandleClientMessageFromService(string message)
        {
            RawChatMessageFromClient? MFS= JsonSerializer.Deserialize<RawChatMessageFromClient>(message);
            if(MFS!= null)
            {
                string to;
                if (MFS.To == ChatType.Private)
                {
                    if (MFS.From == player.Username)
                    {
                        string? otherplayerUsername = player.gameServer.GetOtherPlayerUsername(player);
                        if (otherplayerUsername != null) { to = otherplayerUsername; }
                        else
                            to = "";
                    }
                        
                    else
                        to = player.Username;
                }
                   
                else to = player.gameServer.LobbyID;

                if(MFS.To==ChatType.Private)
                {
                     ;
                }

                RawChatMessage messageToSend = new RawChatMessage(MFS.From, to, MFS.Message);
                SendMessage(messageToSend);
            }
           


        }

        #endregion
        #region Public
        Thread PublicChatThread;
        private bool SetupPublicChatListner()
        {
            try
            {
                if (IP != null)
                {
                    GameServer server = player.gameServer;
                    List<string> pkeys = new List<string>() { "" };
                    Consumer.HandleMessageDelegate handler = HandlePublicMessageFromService;
                    PublicChatThread = new(() => Consumer.ListenToChat(this, IP, Producer.ChatGroup.Public, pkeys, handler)) { IsBackground = true };
                    PublicChatThread.Start();
                    PrintThread(Producer.ChatGroup.Public, pkeys);


                }
                return true;
            }
            catch (Exception e)
            {
                Print("Failed when setting up Public chat Listner: " + e);
                return false;
            }
        }

        private void HandlePublicMessageFromService(string message)
        {
            //RawChatMessageFromClient? MFS = JsonSerializer.Deserialize<RawChatMessageFromClient>(message);
            RawChatMessage messageToSend = new RawChatMessage("ADMIN", "ALL", message);
            SendMessage(messageToSend);
        }
        #endregion

        #endregion

        #region --------------------------Sending (Producing)-----------------------
        public void ProcessChatMessageFromClient(RawChatMessageFromClient cm)
        {
            if(IP != null)
            {
                ChatType type = cm.To;
                string from = cm.From;

                string message = JsonSerializer.Serialize(cm);
                if (type == ChatType.Private)
                {
                    Print("Sending Private Message To Chat Service: "+message);
                    GameServer server = player.gameServer;
                    //string key =server.address + ":" + server.port;
                    string key = server.server.LocalEndpoint + "";
                    Producer.ConnectToChatServiceAndSend(IP, Producer.ChatGroup.Private, key,message);
                    PrintSend(Producer.ChatGroup.Private, key, message);
                }
                else if (type == ChatType.Group)
                {
                    Print("Sending Group Message To Chat Service: "+message);
                    GameServer server = player.gameServer;
                    string key = server.LobbyID;
                    Producer.ConnectToChatServiceAndSend(IP, Producer.ChatGroup.Group, key, message);
                    PrintSend(Producer.ChatGroup.Group, key, message);
                }
            }      
        }
        public static void SendPublicMessageToService(string message)
        {
            ServerPrint("Sending Public Message to Chat Service: " + message);
            try
            {

                Settings.Settings.LoadSettings();
                Producer.ConnectToChatServiceAndSend(Settings.Settings.ServerSettings.ChatServiceIP, Producer.ChatGroup.Public, null, message);
                //PrintSend(Producer.ChatGroup.Public, null, message);
            }
            catch   (Exception e)
            {
                ServerPrint("Couldnt Send Public Message: " + e);
            }


        }
        #endregion

        void Print(string msg)
        {
            player.Print(">[ChatServiceInterface]: "+msg);
        }
        void PrintThread(Producer.ChatGroup chatGroup,List<string> routingKeys)
        {
            var chat=Producer.ChatGroups[chatGroup];
            StringBuilder sb=   new StringBuilder();

            sb.AppendLine("Now Listening To:");
            sb.AppendLine("Chat: "+chat.Name);
            sb.AppendLine("Routing Keys:");
            foreach(string key in routingKeys)
            {
                sb.AppendLine("-" + key);
            }
            string message=sb.ToString();
            
            Print(message);
            
        }
        void PrintSend(Producer.ChatGroup chatGroup, string key,string msg)
        {
            string name = Producer.ChatGroups[chatGroup].Name;
            Print(
                $"Send Message      : {msg}" +
                $"\nTo              : {name}" +
                $"\nUsing The Key   : {key}");
        }
        static void  ServerPrint(string msg)
        {
            Console.WriteLine("[GameServer]: >[ChatServiceInterface]"+msg);
        }
    }
}
