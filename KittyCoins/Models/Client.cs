namespace KittyCoins.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ViewModels;
    using Newtonsoft.Json;
    using WebSocketSharp;
    using System.Threading;

    /// <summary>
    /// The client class
    /// Used to send packets to other server
    /// </summary>
    public class Client
    {
        #region Public Attributes

        public static EventHandler NewMessage { get; set; }

        public static EventHandler BlockchainUpdate { get; set; }

        public string ServerAddress { get; set; }

        #endregion

        #region Constructor

        public Client()
        {
            // Create the dictionnary who will contain the servers address
            MainViewModel.ServerList = new Dictionary<string, WebSocket>();
        }

        #endregion

        #region Public Methods

        public void Connect(string url)
        {
            // If we know the adress (url) or if it's our server address don't connect
            if (MainViewModel.ServerList.ContainsKey(url) || url == ServerAddress) return;

            // Message for the console
            NewMessage.Invoke(this, new EventArgsMessage($"Begin Connection to {url}"));

            // Create the webSocket from the url
            var ws = new WebSocket(url);
            ws.OnMessage += (sender, e) =>
            {
                try
                {
                    var guid = Guid.NewGuid();
                    MainViewModel.BlockChainWaitingList.Add(guid);
                    while (!MainViewModel.BlockChainAccessToken &&
                            MainViewModel.BlockChainWaitingList.First().Equals(guid))
                    {
                        Thread.Sleep(50);
                    }

                    MainViewModel.BlockChainAccessToken = false;
                    MainViewModel.BlockChainWaitingList.Remove(guid);

                    #region BlockChain Received

                    if (e.Data.StartsWith(Constants.BLOCKCHAIN_IS_NOT_VALID))
                    {
                        NewMessage.Invoke(this, new EventArgsMessage(Constants.BLOCKCHAIN_IS_NOT_VALID));
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(Constants.BLOCKCHAIN_IS_NOT_VALID.Length));
                        BlockchainUpdate.Invoke(this, new EventArgsObject(chainReceived));
                        return;
                    }
                    else if (e.Data.StartsWith(Constants.BLOCKCHAIN_MISS_BLOCK))
                    {
                        NewMessage.Invoke(this, new EventArgsMessage(Constants.BLOCKCHAIN_MISS_BLOCK));
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(Constants.BLOCKCHAIN_MISS_BLOCK.Length));
                        BlockchainUpdate.Invoke(this, new EventArgsObject(chainReceived));
                        return;
                    }
                    else if (e.Data.StartsWith(Constants.BLOCKCHAIN_OVERWRITE))
                    {
                        NewMessage.Invoke(this, new EventArgsMessage(Constants.BLOCKCHAIN_OVERWRITE));
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(Constants.BLOCKCHAIN_OVERWRITE.Length));
                        BlockchainUpdate.Invoke(this, new EventArgsObject(chainReceived));
                        return;
                    }

                    #endregion

                    #region Server List Request Receive

                    // The request want our server list
                    else if (e.Data.StartsWith("ServerList"))
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("Server list received"));

                        // Deserialize the server send in the request
                        // The Substring cut "GetServers"
                        var servers = JsonConvert.DeserializeObject<List<string>>(e.Data.Substring(10));

                        ConnectToAll(servers);
                    }

                    #endregion

                    // Unknow request
                    else
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("Unknown message"));
                    }
                }
                catch (Exception)
                {
                    NewMessage.Invoke(this, new EventArgsMessage("Impossible to deserialize received object"));
                }
                finally
                {
                    MainViewModel.BlockChainAccessToken = true;
                }
            };
            ws.Connect();
            MainViewModel.ServerList.Add(url, ws);
            ws.Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
            ws.Send("GetServers" + JsonConvert.SerializeObject(new List<string>(GetServers()) { ServerAddress }));
        }

        /// <summary>
        /// Send a message (data) to a server (url)
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        public void Send(string url, string data)
        {
            // If the message (data) or the receiver (url) is null don't send
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(data)) return;

            // If we know the adress send the data
            if (MainViewModel.ServerList.ContainsKey(url))
                MainViewModel.ServerList[url].Send(data);
        }

        /// <summary>
        /// Send a message (data) to all server in the dictionnary (wsDict)
        /// </summary>
        /// <param name="data"></param>
        public void Broadcast(string data)
        {
            foreach (var item in MainViewModel.ServerList)
            {
                item.Value.Send(data);
            }
        }

        /// <summary>
        /// Return the list of server url
        /// </summary>
        public IList<string> GetServers()
        {
            return MainViewModel.ServerList.Select(item => item.Key).ToList();
        }

        /// <summary>
        /// Close all connection
        /// </summary>
        public void Close()
        {
            foreach (var item in MainViewModel.ServerList)
            {
                item.Value.Close();
            }
        }

        #endregion

        public void NewBlock(Block block)
        {
            Broadcast("Block" + JsonConvert.SerializeObject(block));
        }

        public void NewTransfer(Transfer transfer)
        {
            Broadcast("Transfer" + JsonConvert.SerializeObject(transfer));
        }

        public void ConnectToAll(List<string> servers)
        {
            // If they have server that we didn't know in the request
            if (!MainViewModel.ServerList.Keys.Except(servers).Any())
            {
                NewMessage.Invoke(this, new EventArgsMessage("Connect to the others servers"));

                // Connect to those servers
                foreach (var address in servers)
                {
                    NewMessage.Invoke(this, new EventArgsMessage($"Connect to {address}"));
                    Connect(address);
                }
            }
        }
    }
}