using System;
using System.Collections.Generic;
using System.Linq;
using KittyCoin.ViewModels;
using Newtonsoft.Json;
using WebSocketSharp;

namespace KittyCoin.Models
{
    /// <summary>
    /// The client class
    /// Used to send packets to other server
    /// </summary>
    public class Client
    {
        #region Public Attributes

        /// <summary>
        /// Event Handler use to inform that they have a new message to show at the user
        /// </summary>
        public static EventHandler NewMessage { get; set; }

        /// <summary>
        /// The actual IP Address of the server
        /// </summary>
        /// <example>
        /// ws://192.168.1.10:6002/Blockchain
        /// </example>
        public string ServerAddress { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// The Client constructor
        /// Initialize the server list with an empty list
        /// </summary>
        public Client()
        {
            // Create the dictionnary who will contain the servers address
            MainViewModel.ServerList = new Dictionary<string, WebSocket>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Method used to connect to a server
        /// It will :
        /// - Create the "OnMessage" method to answer the server request
        /// - Send the blockchain in local to the server (to check who have the good one)
        /// - Ask for new server
        /// </summary>
        /// <param name="url">
        /// The server address
        /// </param>
        /// <remarks>
        /// It will create a connection only if the server is not the local one
        /// and if the server is not already in the server list
        /// </remarks>
        public void Connect(string url)
        {
            // If we know the adress (url) or if it's our server address don't connect
            if (MainViewModel.ServerList.ContainsKey(url) || url == ServerAddress) return;

            // Message for the console
            NewMessage.BeginInvoke(this, new EventArgsMessage($"Begin Connection to {url}"), null, null);

            // Create the webSocket from the url
            var ws = new WebSocket(url);
            ws.OnMessage += (sender, e) =>
            {
                var guid = Guid.NewGuid();
                try
                {
                    MainViewModel.WaitingForBlockchainAccess(guid);

                    #region BlockChain Received

                    if (e.Data.StartsWith(Constants.BLOCKCHAIN_IS_NOT_VALID))
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage(Constants.BLOCKCHAIN_IS_NOT_VALID), null, null);
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(Constants.BLOCKCHAIN_IS_NOT_VALID.Length));
                        MainViewModel.BlockChain = chainReceived;
                        MainViewModel.BlockChain.PendingTransfers = new List<Transfer>();
                        NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain updated from server"), null, null);
                    }
                    else if (e.Data.StartsWith(Constants.BLOCKCHAIN_MISS_BLOCK))
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage(Constants.BLOCKCHAIN_MISS_BLOCK), null, null);
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(Constants.BLOCKCHAIN_MISS_BLOCK.Length));
                        MainViewModel.BlockChain = chainReceived;
                        MainViewModel.BlockChain.PendingTransfers = new List<Transfer>();
                        NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain updated from server"), null, null);
                    }
                    else if (e.Data.StartsWith(Constants.BLOCKCHAIN_OVERWRITE))
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage(Constants.BLOCKCHAIN_OVERWRITE), null, null);
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(Constants.BLOCKCHAIN_OVERWRITE.Length));
                        MainViewModel.BlockChain = chainReceived;
                        MainViewModel.BlockChain.PendingTransfers = new List<Transfer>();
                        NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain updated from server"), null, null);
                    }
                    else if (e.Data.StartsWith(Constants.NEED_BLOCKCHAIN))
                    {
                        ws.Send(Constants.BLOCKCHAIN + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }
                    else if (e.Data.StartsWith(Constants.BLOCKCHAIN))
                    {
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(Constants.BLOCKCHAIN.Length));
                        if (!MainViewModel.BlockChain.IsValid() && chainReceived.IsValid())
                        {
                            MainViewModel.BlockChain = chainReceived;
                            MainViewModel.BlockChain.PendingTransfers = new List<Transfer>();
                            NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain updated from server"), null, null);
                        }
                    }

                    #endregion

                    #region Server List Request Receive

                    // The request want our server list
                    else if (e.Data.StartsWith("ServerList"))
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("Server list received"), null, null);

                        // Deserialize the server send in the request
                        // The Substring cut "GetServers"
                        var servers = JsonConvert.DeserializeObject<List<string>>(e.Data.Substring(10));

                        ConnectToAll(servers);
                    }

                    #endregion

                    // Unknow request
                    else
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("Unknown message"), null, null);
                    }
                }
                catch (Exception ex)
                {
                    NewMessage.BeginInvoke(this, new EventArgsMessage(ex.Message), null, null);
                }
                finally
                {
                    MainViewModel.BlockChainWaitingList.Remove(guid);
                    var receivers = MainViewModel.BlockChainUpdated?.GetInvocationList();
                    if (receivers != null)
                    {
                        foreach (EventHandler receiver in receivers)
                        {
                            receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                        }
                    }
                }
            };
            ws.Connect();
            MainViewModel.ServerList.Add(url, ws);
            var receiversServerList = MainViewModel.ServerListUpdated?.GetInvocationList();
            if (receiversServerList != null)
            {
                foreach (EventHandler receiver in receiversServerList)
                {
                    receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                }
            }
            ws.Send(Constants.BLOCKCHAIN + JsonConvert.SerializeObject(MainViewModel.BlockChain));
            ws.Send(Constants.GET_SERVERS + JsonConvert.SerializeObject(new List<string>(GetServers()) { ServerAddress }));
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
        /// <remarks>
        /// It will close connection with server who didn't respond
        /// </remarks>
        public void Broadcast(string data)
        {
            var serverClose = new Dictionary<string, WebSocket>();
            foreach (var item in MainViewModel.ServerList)
            {
                try
                {
                    item.Value.Send(data);
                }
                catch (Exception)
                {
                    NewMessage.BeginInvoke(this, new EventArgsMessage($"The server {item.Key} is closed."), null, null);
                    serverClose.Add(item.Key, item.Value);
                }
            }
            foreach (var item in serverClose)
            {
                MainViewModel.ServerList.Remove(item);
                var receivers = MainViewModel.ServerListUpdated?.GetInvocationList();
                if (receivers != null)
                {
                    foreach (EventHandler receiver in receivers)
                    {
                        receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                    }
                }
            }
        }

        /// <summary>
        /// Return the list of server ip
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

        /// <summary>
        /// Broadcast the created block
        /// </summary>
        /// <param name="block"></param>
        /// <see cref="Broadcast"/>
        public void NewBlock(Block block)
        {
            Broadcast(Constants.BLOCK + JsonConvert.SerializeObject(block));
        }

        /// <summary>
        /// Ask a server to send his blockchain
        /// </summary>
        public void NeedBlockchain()
        {
            foreach (var item in MainViewModel.ServerList)
            {
                try
                {
                    item.Value.Send(Constants.NEED_BLOCKCHAIN);
                    break;
                }
                catch (Exception)
                { }
            }
        }

        /// <summary>
        /// Broadcast the transaction
        /// </summary>
        /// <param name="transfer"></param>
        /// <see cref="Broadcast"/>
        public void NewTransfer(Transfer transfer)
        {
            Broadcast(Constants.TRANSFER + JsonConvert.SerializeObject(transfer));
        }

        /// <summary>
        /// Connect to a list of server
        /// </summary>
        /// <param name="servers"></param>
        /// <see cref="Connect"/>
        public void ConnectToAll(List<string> servers)
        {
            NewMessage.BeginInvoke(this, new EventArgsMessage("Connect to the others servers"), null, null);

            // Connect to those servers
            foreach (var address in servers)
            {
                Connect(address);
            }
        }
    }
}