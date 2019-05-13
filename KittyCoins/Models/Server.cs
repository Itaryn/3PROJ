using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using KittyCoins.ViewModels;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KittyCoins.Models
{
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
        public string ServerAddress = "";

        public static EventHandler NewMessage { get; set; }

        public static EventHandler BlockchainUpdate { get; set; }

        public static EventHandler ServerUpdate { get; set; }

        public List<string> ServersList { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the server
        /// </summary>
        /// <param name="port"></param>
        public string Start(int port, string ip = "127.0.0.1")
        {
            foreach (var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                try
                {
                    // Set the web socket at the local address
                    wss = new WebSocketServer($"{Constants.SERVER_ADDRESS}{address}:{port}");
                    ip = address.ToString();
                    break;
                }
                catch (Exception)
                {

                }
            }
            if (wss == null)
            {
                new WebSocketServer($"{Constants.SERVER_ADDRESS}{ip}:{port}");
            }

            // Set the service "Blockchain"
            wss.AddWebSocketService<Server>(Constants.WEB_SERVICE_NAME);
            wss.Start();

            // Set the address
            ServerAddress = $"{Constants.SERVER_ADDRESS}{ip}:{port}{Constants.WEB_SERVICE_NAME}";

            return ServerAddress;
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Method called when the server receive a message
        /// </summary>
        /// <param name="e"></param>
        protected override void OnMessage(MessageEventArgs e)
        {
            var guid = Guid.NewGuid();
            try
            {
                MainViewModel.BlockChainWaitingList.Add(guid);
                while (!MainViewModel.BlockChainWaitingList.FirstOrDefault().Equals(guid))
                {
                    if (MainViewModel.BlockChainWaitingList.FirstOrDefault().Equals(new Guid()))
                    {
                        MainViewModel.BlockChainWaitingList.Remove(new Guid());
                    }
                }

                #region BlockChain Receive

                // The request send the entire blockchain
                if (e.Data.StartsWith("BlockChain"))
                {
                    // Deserialize the blockchain received
                    // The Substring cut "BlockChain" or "BlockChainOverwrite"
                    var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(e.Data.StartsWith("BlockChainOverwrite") ? 19 : 10));
                    NewMessage.Invoke(this, new EventArgsMessage("Check blockchain"));

                    /* If chain received and local is not valid
                     * OR
                     * If same blockchain received and local
                     * OR
                     * If same chain and same pending transfers list
                     * => Do nothing
                     */
                    if (MainViewModel.BlockChain.Equals(chainReceived))
                    {
                        MainViewModel.BlockChainWaitingList.Remove(guid);
                        return;
                    }

                    if (!MainViewModel.BlockChain.IsValid() &&
                        !chainReceived.IsValid())
                    {
                        MainViewModel.BlockChain.InitializeChain();
                    }

                    // If chain received is not valid but local is
                    // => Send that the received blockchain is not valid
                    if (!chainReceived.IsValid() && MainViewModel.BlockChain.IsValid())
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("Blockchain receive not valid but local is"));
                        Send(Constants.BLOCKCHAIN_IS_NOT_VALID + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }

                    // If chain received is valid but local is not
                    // Copy the received blockchain
                    if (!chainReceived.IsValid() && MainViewModel.BlockChain.IsValid())
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("Blockchain receive is valid and local is not"));
                        MainViewModel.BlockChain = chainReceived;
                        NewMessage.Invoke(this, new EventArgsMessage("BlockChain updated from server"));
                    }

                    // If the received chain is bigger than local
                    // => Copy the received blockchain
                    else if (chainReceived.Chain.Count > MainViewModel.BlockChain.Chain.Count)
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("Blockchain is bigger than local"));
                        MainViewModel.BlockChain = chainReceived;
                        NewMessage.Invoke(this, new EventArgsMessage("BlockChain updated from server"));
                    }

                    // If the received chain is lower than local
                    // => Send the local blockchain
                    else if (chainReceived.Chain.Count < MainViewModel.BlockChain.Chain.Count)
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("Blockchain receive lower than local"));
                        Send(Constants.BLOCKCHAIN_MISS_BLOCK + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }

                    // If the chain are equals but the pending transfer list are different
                    // => Get the pending transfer not in local and send the list of transfer
                    else if (chainReceived.Chain.SequenceEqual(MainViewModel.BlockChain.Chain) &&
                             !chainReceived.PendingTransfers.SequenceEqual(MainViewModel.BlockChain.PendingTransfers))
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("Chain equals but different pending transfers/nWaiting for transfer message"));
                    }
                    else
                    {
                        // If the sender force to overwrite the local
                        if (e.Data.StartsWith(Constants.BLOCKCHAIN_OVERWRITE))
                        {
                            NewMessage.Invoke(this, new EventArgsMessage("Overwrite BlockChain from sender"));
                            MainViewModel.BlockChain = chainReceived;
                            NewMessage.Invoke(this, new EventArgsMessage("BlockChain updated from server"));
                        }
                        // Send a overwrite force to the sender
                        else
                        {
                            NewMessage.Invoke(this, new EventArgsMessage("BlockChain receive is same size than actual but different information"));
                            Send(Constants.BLOCKCHAIN_OVERWRITE + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                        }
                    }
                }

                #endregion
                
                #region Block Receive

                else if (e.Data.StartsWith("Block"))
                {
                    if (!MainViewModel.BlockChain.IsValid())
                    {
                        Send(Constants.NEED_BLOCKCHAIN);
                    }
                    else
                    {
                        // Deserialize the block
                        // The Substring cut "Block"
                        var newBlock = JsonConvert.DeserializeObject<Block>(e.Data.Substring(5));
                        NewMessage.Invoke(this, new EventArgsMessage("New Block received"));

                        if (newBlock.PreviousHash == MainViewModel.BlockChain.Chain.Last().Hash)
                        {
                            newBlock.Index = MainViewModel.BlockChain.LastBlock.Index + 1;
                            MainViewModel.BlockChain.Chain.Add(newBlock);
                            foreach (var tr in newBlock.Transfers)
                            {
                                MainViewModel.BlockChain.PendingTransfers.Remove(tr);
                            }

                            if (newBlock.Index % Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY == 0)
                            {
                                NewMessage.Invoke(this, new EventArgsMessage(MainViewModel.BlockChain.CheckDifficulty()));
                            }
                        }
                    }
                }

                #endregion

                #region Transfer Receive

                // The request send a new transfer
                else if (e.Data.StartsWith("Transfer"))
                {
                    // Deserialize the transfer
                    // The Substring cut "Transfer"
                    var newTransfer = JsonConvert.DeserializeObject<Transfer>(e.Data.Substring(8));
                    NewMessage.Invoke(this, new EventArgsMessage("New transfer received"));

                    // If we already have it or it's not a valid transfer don't add it
                    if (MainViewModel.BlockChain.PendingTransfers.Contains(newTransfer) || !newTransfer.IsValid())
                    {
                        NewMessage.Invoke(this, new EventArgsMessage("New Transfer not valid or already in local"));
                    }
                    else
                    {
                        // If already is Ok add it to our pending transfer list
                        MainViewModel.BlockChain.PendingTransfers.Add(newTransfer);
                        NewMessage.Invoke(this, new EventArgsMessage("New transfer added from server"));
                    }
                }
                // The request send a list of transfer
                else if (e.Data.StartsWith("Transfers"))
                {
                    // Deserialize the transfer
                    // The Substring cut "Transfer"
                    var newTransfers = JsonConvert.DeserializeObject<List<Transfer>>(e.Data.Substring(9));
                    NewMessage.Invoke(this, new EventArgsMessage("New list of transfer received"));

                    foreach (var newTransfer in newTransfers)
                    {
                        // If we already have it or it's not a valid transfer don't add it
                        if (MainViewModel.BlockChain.PendingTransfers.Contains(newTransfer) || !newTransfer.IsValid())
                        {
                            NewMessage.Invoke(this, new EventArgsMessage("New Transfer not valid or already in local"));
                        }
                        else
                        {
                            // If already is Ok add it to our pending transfer list
                            MainViewModel.BlockChain.PendingTransfers.Add(newTransfer);
                            NewMessage.Invoke(this, new EventArgsMessage("New transfer added from server"));
                        }
                    }
                }

                #endregion

                #region GetServers Receive

                else if (e.Data.StartsWith("GetServers"))
                {
                    NewMessage.Invoke(this, new EventArgsMessage("Get Servers request received"));
                    var listWs = JsonConvert.DeserializeObject<List<string>>(e.Data.Substring(10));

                    ServerUpdate.Invoke(this, new EventArgsObject(listWs));
                    if (MainViewModel.ServerList.Any())
                    {
                        Send("ServerList" + JsonConvert.SerializeObject(MainViewModel.ServerList.Select(x => x.Key)));
                    }
                }

                #endregion

                else
                {
                    NewMessage.Invoke(this, new EventArgsMessage("Unknown message"));
                }
            }
            catch (Exception ex)
            {
                NewMessage.Invoke(this, new EventArgsMessage("Impossible to deserialize received object"));
            }
            finally
            {
                MainViewModel.BlockChainWaitingList.Remove(guid);
                MainViewModel.BlockChainUpdated?.Invoke(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
