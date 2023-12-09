using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;

namespace GameServer.Client_Facing.Messages
{
    public class ClientMessageHandler
    {
        TcpClient client;
        Player player;
        private NetworkStream stream;

        Thread listenToClientThread;
        
        ShotMessage? latestShotMessage;
       public ShotMessage? LatestShotMessage
        {
            get
            {
                if(latestShotMessage == null)
                {
                    return latestShotMessage;
                }
                else
                {
                    ShotMessage shotMessage = latestShotMessage;
                    latestShotMessageMutex.WaitOne();
                    latestShotMessage = null;
                    latestShotMessageMutex.ReleaseMutex();
                    return shotMessage;
                }

            }
            private set
            {
                latestShotMessageMutex.WaitOne();
                latestShotMessage = value;
                latestShotMessageMutex.ReleaseMutex();
            }
        }
        Mutex latestShotMessageMutex = new Mutex();
        
        
        
        public ClientMessageHandler(Player _player) 
        {
            player= _player;
            client= player.client;

            //LatestShotMessage = null;
        }


        
        public void StartListeningForMessages(NetworkStream stream)
        {
            player.Print("Setting Up Listening Loop");
            this.stream = stream;
            //stream = client.GetStream();
            listenToClientThread = new Thread(() => RecieveDataLinesFromClient(stream)) { IsBackground = true };
            listenToClientThread.Start();

        }
        // TODO: SOFIE This Needs A Test with half Mock Client 
        public void RecieveDataFromClient(NetworkStream stream)
        {
            throw new Exception("We dont Want to Recieve Messages like this");
            try
            {
                while (client.Connected)
                {
                    StringBuilder sb = new StringBuilder();
                    byte[] tempBytes = new byte[1];
                    while (true && client.Connected)
                    {
                        int bytesRead = stream.Read(tempBytes);
                        if (bytesRead == 0) { break; }
                        string recievedData = Encoding.UTF8.GetString(tempBytes);
                        if (player.gameServer.useEndMessage)
                        {
                            if (recievedData.Contains(GameServer.END_OF_MESSAGE)) { break; }
                            sb.Append(recievedData);
                        }
                        else
                        {
                            sb.Append(recievedData);
                            break;
                        }


                    }
                    string message = sb.ToString();

                    SortClientMessage(message);
                }
            }
            catch (Exception e)
            {
                player.Print("Error Occured when Listening for MEssages:\n"+e);
                //throw;
            }
            


        }
        public void RecieveDataLinesFromClient(NetworkStream stream)
        {
            try
            {
                StreamReader reader = new(stream);

                //string message = reader.ReadLine() ?? "";
                //return message;
                
                while (client.Connected)
                {
                    player.Print("Listening For Message");
                    //string message = reader.ReadLine() ?? "";
                    string? message = reader.ReadLine();
                    if(message!=null)
                    {
                        player.Print($"Got Message: [{message}]");
                        SortClientMessage(message);
                    }
                    else
                    {
                        player.Print("WARNING: Player got an null message We assume this Means the Player Has Disconnected and reflect that by disconnecting client");
                        client.Close();
                    }
                       
                    
                }
                player.Print("Listen For Message Loop Broken");
            }
            catch (Exception e)
            {
                player.Print("Error Occured when Listening for MEssages:\n" + e);
                //throw;
            }
            


        }
        public string SortClientMessage(string message)
        {
            player.Print("Sorting Message: " + message);
            player.Print("Client Is Connected: " + client.Connected);
            string returnString = "";
            IClientMessage? temp=null;
            Testing.Print("[ClientMessageHandler]SortClientMessage> Deserializing into Shot Message");
            temp = DeserializeMessage<ShotMessage>(message);
            if(temp!= null /*&& temp.GetType()==Type.GetType("ShotMessage")*/)
            {
                Testing.Print("[ClientMessageHandler]SortClientMessage> Checking if Valid Shot Message");
                if (((ShotMessage)temp).ValidShotMessage)
                {
                    Testing.Print("[ClientMessageHandler]SortClientMessage> IsValid Shot Message");
                    returnString = "ShotMessage";
                    HandleShotMessage((ShotMessage)temp);    
                    
                }
                else
                {
                    
                    Testing.Print("[ClientMessageHandler]SortClientMessage> Isnt Valid Shot Message");
                    temp = null;
                }
            }
            if(temp==null /*&& temp.GetType() == Type.GetType("RawChatMessageFromClient")*/)
            {
                Testing.Print("[ClientMessageHandler]SortClientMessage> Deserializing into Chat Message");
                temp = DeserializeMessage<RawChatMessageFromClient>(message);
                if(temp!= null )
                {
                    Testing.Print("[ClientMessageHandler]SortClientMessage> Checking If Valid Chat Message");
                    if (((RawChatMessageFromClient)temp).ValidRawChatMessageFromClient)
                    {
                        Testing.Print("[ClientMessageHandler]SortClientMessage> Is Valid Chat Message");
                        returnString = "RawChatMessageFromClient";

                        HandleChatMessage((RawChatMessageFromClient)temp);
                    }
                    else
                    {
                        Testing.Print("[ClientMessageHandler]SortClientMessage> Isnt Valid Chat Message, setting Temp to null");
                        temp = null;
                    }
                }
                else
                {
                    Testing.Print("[ClientMessageHandler]SortClientMessage> Deserializing into Chat Message Returned Null, continuing on");

                }

            }
            if(temp==null)
            {
                Testing.Print("[ClientMessageHandler]SortClientMessage> Temp Was Null, So Set return string to InvalidMessage");
                returnString = "InvalidMessage";
            }

            Testing.Print("[ClientMessageHandler]SortClientMessage> Returning String");
            return returnString;


        }
        public IClientMessage? DeserializeMessage<T>(string message) where T : IClientMessage 
        {
            try
            {
                return JsonSerializer.Deserialize<T>(message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error When Deserializing Message From Client: " + e);
                return null;
            }
        }
        public void HandleShotMessage(ShotMessage shotMessage)
        {
            LatestShotMessage=shotMessage;
        }

        Mutex chatMessageMutex = new Mutex();
        
        public Queue<RawChatMessageFromClient> ChatMessages { get; private set; } = new();

        public void HandleChatMessage(RawChatMessageFromClient rawChatMessageFromClient)
        {
            //AddChatMessageToQueue(rawChatMessageFromClient);
            try
            {
                player.ChatServiceInterface.ProcessChatMessageFromClient(rawChatMessageFromClient);
            }
            catch(Exception e)
            {
                player.Print("Couldnt Handle Chat Message: " + e);
            }
            

            
        }
        //public void AddChatMessageToQueue(RawChatMessageFromClient message)
        //{
        //    chatMessageMutex.WaitOne();
        //    ChatMessages.Enqueue(message);
        //    chatMessageMutex.ReleaseMutex();
        //}
        //// 
        //public RawChatMessageFromClient GetChatMessageFromQueue()
        //{
        //    chatMessageMutex.WaitOne();
        //    var message = ChatMessages.Dequeue();
        //    chatMessageMutex.ReleaseMutex();
        //    return message;
        //}




        #region TESTING
        public void TEST_Set_LatestMessage(ShotMessage? sm)
        {
            if(Testing.IsTesting)
            {
                LatestShotMessage = sm;
            }
        }
        #endregion

    }
}
