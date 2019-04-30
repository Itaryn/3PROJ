namespace KittyCoins.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ViewModels;
    using Newtonsoft.Json;
    using WebSocketSharp;

    /// <summary>
    /// The client class
    /// Used to send packets to other server
    /// </summary>
    public class Client
    {
        #region Constructor

        public Client()
        {
            // Add a message in the console
            MainViewModel.MessageFromClientOrServer.Add("Create Client from client");

            // Create the dictionnary who will contain the servers address
            MainViewModel.WsDict = new Dictionary<string, WebSocket>();
        }

        #endregion

        #region Public Methods

        public void Connect(string url)
        {
            // If we know the adress (url) or if it's our server address don't connect
            if (MainViewModel.WsDict.ContainsKey(url) || url == Server.serverAddress) return;

            // Message for the console
            MainViewModel.MessageFromClientOrServer.Add($"Begin Connection to {url}");

            // Create the webSocket from the url
            var ws = new WebSocket(url);
            ws.OnMessage += (sender, e) =>
            {
                try
                {
                    #region BlockChain Receive

                    // The request send the entire blockchain
                    if (e.Data.StartsWith("BlockChain"))
                    {
                        // Deserialize the blockchain received
                        // The Substring cut "BlockChain" or "BlockChainOverwrite"
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(e.Data.StartsWith("BlockChainOverwrite") ? 19 : 10));
                        MainViewModel.MessageFromClientOrServer.Add("Check blockchain");

                        /* If chain received and local is not valid
                         * OR
                         * If same blockchain received and local
                         * OR
                         * If same chain and same pending transfers list
                         * => Do nothing
                         */
                        if (!chainReceived.IsValid() && !MainViewModel.BlockChain.IsValid() ||
                            MainViewModel.BlockChain.Equals(chainReceived)) return;

                        // If chain received is not valid but local is
                        // => Send local blockchain in response
                        if (!chainReceived.IsValid() && MainViewModel.BlockChain.IsValid())
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Blockchain receive not valid but local is");
                            Send(ws.Origin, "BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                        }

                        // If the received chain is bigger than local
                        // => Copy the received blockchain and send the list of transfer
                        else if (chainReceived.Chain.Count > MainViewModel.BlockChain.Chain.Count)
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Blockchain is bigger than local");

                            chainReceived.PendingTransfers.AddRange(MainViewModel.BlockChain.PendingTransfers.Except(chainReceived.PendingTransfers));
                            MainViewModel.BlockChain = chainReceived;

                            Send(ws.Origin, "Transfers" + JsonConvert.SerializeObject(MainViewModel.BlockChain.PendingTransfers));
                        }

                        // If the received chain is lower than local
                        // => Send the local blockchain
                        else if (chainReceived.Chain.Count < MainViewModel.BlockChain.Chain.Count)
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Blockchain receive lower than local");
                            Send(ws.Origin, "BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                        }

                        // If the chain are equals but the pending transfer list are different
                        // => Get the pending transfer not in local and send the list of transfer
                        else if (chainReceived.Chain.SequenceEqual(MainViewModel.BlockChain.Chain) &&
                                 !chainReceived.PendingTransfers.SequenceEqual(MainViewModel.BlockChain.PendingTransfers))
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Chain equals but different pending transfers");
                            MainViewModel.BlockChain.PendingTransfers.AddRange(chainReceived.PendingTransfers.Except(MainViewModel.BlockChain.PendingTransfers));
                            Send(ws.Origin, "Transfers" + JsonConvert.SerializeObject(MainViewModel.BlockChain.PendingTransfers));
                        }
                        else
                        {
                            // If the sender force to overwrite the local
                            if (e.Data.StartsWith("BlockChainOverwrite"))
                            {
                                MainViewModel.MessageFromClientOrServer.Add("Overwrite BlockChain from sender");
                                MainViewModel.BlockChain = chainReceived;
                            }
                            // Send a overwrite force to the sender
                            else
                            {
                                var a = JsonConvert.SerializeObject(MainViewModel.BlockChain);
                                var b = JsonConvert.SerializeObject(chainReceived);
                                MainViewModel.MessageFromClientOrServer.Add("BlockChain receive is same size than local but different information");
                                Send(ws.Origin, "BlockChainOverwrite" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                            }
                        }
                    }

                    #endregion

                    #region GetServer Request Receive

                    // The request want our server list
                    else if (e.Data.StartsWith("GetServers"))
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Get Servers request received");

                        // Deserialize the server send in the request
                        // The Substring cut "GetServers"
                        var listWs = JsonConvert.DeserializeObject<List<string>>(e.Data.Substring(10));
                        
                        // If they have server that we didn't know in the request
                        if (!MainViewModel.WsDict.Keys.Except(listWs).Any())
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Connect to the others servers");

                            // Connect to those servers
                            foreach (var address in listWs.Except(MainViewModel.WsDict.Keys))
                                MainViewModel.Client.Connect(address);

                            // Answer with our server
                            Send(ws.Origin, "GetServers" + JsonConvert.SerializeObject(new List<string>(MainViewModel.WsDict.Keys) { Server.serverAddress }));
                        }
                    }

                    #endregion

                    // Unknow request
                    else
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Unknown message");
                    }
                }
                catch (Exception)
                {
                    MainViewModel.MessageFromClientOrServer.Add("Impossible to deserialize received object");
                }
            };
            ws.Connect();
            MainViewModel.WsDict.Add(url, ws);
            ws.Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
            ws.Send("GetServers" + JsonConvert.SerializeObject(new List<string>(MainViewModel.Client.GetServers()) { Server.serverAddress }));
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
            if (MainViewModel.WsDict.ContainsKey(url))
                MainViewModel.WsDict[url].Send(data);
        }

        /// <summary>
        /// Send a message (data) to all server in the dictionnary (wsDict)
        /// </summary>
        /// <param name="data"></param>
        public void Broadcast(string data)
        {
            foreach (var item in MainViewModel.WsDict)
            {
                item.Value.Send(data);
            }
        }

        /// <summary>
        /// Return the list of server url
        /// </summary>
        public IList<string> GetServers()
        {
            return MainViewModel.WsDict.Select(item => item.Key).ToList();
        }

        /// <summary>
        /// Close all connection
        /// </summary>
        public void Close()
        {
            foreach (var item in MainViewModel.WsDict)
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
    }
}