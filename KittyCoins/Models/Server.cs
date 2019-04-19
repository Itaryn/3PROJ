namespace KittyCoins.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ViewModels;
    using Newtonsoft.Json;
    using WebSocketSharp;
    using WebSocketSharp.Server;

    public class Server : WebSocketBehavior
    {
        #region Public Attributes

        /// <summary>
        /// The webSocket
        /// </summary>
        public WebSocketServer wss;

        /// <summary>
        /// The Server Address
        /// </summary>
        public static string serverAddress = "";

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="port"></param>
        public void Start(int port)
        {
            // Set the web socket at the local address
            wss = new WebSocketServer($"ws://127.0.0.1:{port}");

            // Set the service "Blockchain"
            wss.AddWebSocketService<Server>("/Blockchain");
            wss.Start();

            // Set the address
            serverAddress = $"ws://127.0.0.1:{port}/Blockchain";


            MainViewModel.MessageFromClientOrServer.Add($"Started server at {serverAddress}");
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Method called when the server receive a message
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMessage(MessageEventArgs e)
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
                        chainReceived.Equals(MainViewModel.BlockChain) ||
                        chainReceived.Chain.Equals(MainViewModel.BlockChain.Chain) &&
                        chainReceived.PendingTransfers.Equals(MainViewModel.BlockChain.PendingTransfers)) return;

                    // If chain received is not valid but local is
                    // => Send local blockchain in response
                    if (!chainReceived.IsValid() && MainViewModel.BlockChain.IsValid())
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Blockchain receive not valid but actual is");
                        Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }

                    // If the received chain is bigger than local
                    // => Copy the received blockchain and send it
                    else if (chainReceived.Chain.Count > MainViewModel.BlockChain.Chain.Count)
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Blockchain is bigger than local");

                        chainReceived.PendingTransfers.AddRange(MainViewModel.BlockChain.PendingTransfers.Except(chainReceived.PendingTransfers));
                        MainViewModel.BlockChain = chainReceived;

                        Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }

                    // If the received chain is lower than local
                    // => Send the local blockchain
                    else if (chainReceived.Chain.Count < MainViewModel.BlockChain.Chain.Count)
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Blockchain receive lower than local");
                        Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }

                    // If the chain are equals but the pending transfer list are different
                    // => Get the pending transfer not in local and send the blockchain
                    else if (chainReceived.Chain.Equals(MainViewModel.BlockChain.Chain) &&
                             !chainReceived.PendingTransfers.Equals(MainViewModel.BlockChain.PendingTransfers))
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Chain equals but different pending transfers");
                        MainViewModel.BlockChain.PendingTransfers.AddRange(chainReceived.PendingTransfers.Except(MainViewModel.BlockChain.PendingTransfers));
                        Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
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
                            MainViewModel.MessageFromClientOrServer.Add("BlockChain receive is same size than actual but different information");
                            Send("BlockChainOverwrite" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                        }
                    }
                }

                #endregion


                else if (e.Data.StartsWith("Transfer"))
                {
                    var newTransfer = JsonConvert.DeserializeObject<Transfer>(e.Data.Substring(8));
                    MainViewModel.MessageFromClientOrServer.Add("New transfer received");

                    if (MainViewModel.BlockChain.PendingTransfers.Contains(newTransfer) || !newTransfer.IsValid())
                    {
                        MainViewModel.MessageFromClientOrServer.Add("New Transfer not valid or already in local");
                        return;
                    }
                    MainViewModel.BlockChain.PendingTransfers.Add(newTransfer);
                    MainViewModel.MessageFromClientOrServer.Add("New Transfer added");
                }
                else if (e.Data.StartsWith("GetServers"))
                {
                    MainViewModel.MessageFromClientOrServer.Add("Get Servers request received");
                    var listWs = JsonConvert.DeserializeObject<List<string>>(e.Data.Substring(10));

                    if (listWs.Except(MainViewModel.WsDict.Keys).Any())
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Connect to the others servers");

                        foreach (var address in listWs.Except(MainViewModel.WsDict.Keys))
                            MainViewModel.Client.Connect(address);
                    }

                    if (MainViewModel.WsDict.Keys.Except(listWs).Any())
                        Send("GetServers" + JsonConvert.SerializeObject(new List<string>(MainViewModel.Client.GetServers()) { serverAddress }));
                }
                else
                {
                    MainViewModel.MessageFromClientOrServer.Add("Unknown message");
                }
            }
            catch (Exception ex)
            {
                MainViewModel.MessageFromClientOrServer.Add("Impossible to deserialize received object");
                MainViewModel.MessageFromClientOrServer.Add(ex.Message);
            }
        }

        #endregion
    }
}
