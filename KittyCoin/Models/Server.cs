using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using KittyCoin.ViewModels;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KittyCoin.Models
{
    /// <summary>
    /// The Server base on web socket
    /// </summary>
    /// <see cref="WebSocketBehavior"/>
    public class Server : WebSocketBehavior
    {
        #region Public Attributes

        /// <summary>
        /// The Web Socket
        /// </summary>
        public WebSocketServer wss;

        /// <summary>
        /// The Server Address
        /// </summary>
        public string ServerAddress = "";

        /// <summary>
        /// Event Handler use to inform that they have a new message to show at the user
        /// </summary>
        public static EventHandler NewMessage { get; set; }

        /// <summary>
        /// Event Handler use to inform that the server is updated
        /// </summary>
        public static EventHandler ServerUpdate { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Start the server with a port
        /// </summary>
        /// <param name="port"></param>
        public string Start(int port)
        {
            string ip;
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
                { }
            }
            if (wss == null)
            {
                wss = new WebSocketServer($"{Constants.SERVER_ADDRESS}127.0.0.1:{port}");
                ip = "127.0.0.1";
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
                MainViewModel.WaitingForBlockchainAccess(guid);
                #region BlockChain Receive

                // The request send the entire blockchain
                if (e.Data.StartsWith(Constants.BLOCKCHAIN))
                {
                    // Deserialize the blockchain received
                    // The Substring cut "BlockChain" or "BlockChainOverwrite"
                    var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(e.Data.StartsWith("BlockChainOverwrite") ? 19 : 10));
                    NewMessage.BeginInvoke(this, new EventArgsMessage("Check blockchain"), null, null);

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

                    var localIsValid = MainViewModel.BlockChain.IsValid();
                    var receivedIsValid = chainReceived.IsValid();

                    if (!localIsValid &&
                        !receivedIsValid)
                    {
                        MainViewModel.BlockChain.InitializeChain();
                    }

                    // If chain received is not valid but local is
                    // => Send that the received blockchain is not valid
                    if (!receivedIsValid && localIsValid)
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("Blockchain receive not valid but local is"), null, null);
                        Send(Constants.BLOCKCHAIN_IS_NOT_VALID + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }

                    // If chain received is valid but local is not
                    // Copy the received blockchain
                    if (receivedIsValid && !localIsValid)
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("Blockchain receive is valid and local is not"), null, null);
                        MainViewModel.BlockChain = chainReceived;
                        MainViewModel.BlockChain.PendingTransfers = new List<Transfer>();
                        NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain updated from server"), null, null);
                    }

                    // If the received chain is bigger than local
                    // => Copy the received blockchain
                    else if (chainReceived.Chain.Count > MainViewModel.BlockChain.Chain.Count)
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("Blockchain is bigger than local"), null, null);
                        MainViewModel.BlockChain = chainReceived;
                        MainViewModel.BlockChain.PendingTransfers = new List<Transfer>();
                        NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain updated from server"), null, null);
                    }

                    // If the received chain is lower than local
                    // => Send the local blockchain
                    else if (chainReceived.Chain.Count < MainViewModel.BlockChain.Chain.Count)
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("Blockchain receive lower than local"), null, null);
                        Send(Constants.BLOCKCHAIN_MISS_BLOCK + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }

                    // If the chain are equals but the pending transfer list are different
                    // => Get the pending transfer not in local and send the list of transfer
                    else if (chainReceived.Chain.SequenceEqual(MainViewModel.BlockChain.Chain) &&
                             !chainReceived.PendingTransfers.SequenceEqual(MainViewModel.BlockChain.PendingTransfers))
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("Chain equals but different pending transfers/nWaiting for transfer message"), null, null);
                    }
                    else
                    {
                        // If the sender force to overwrite the local
                        if (e.Data.StartsWith(Constants.BLOCKCHAIN_OVERWRITE))
                        {
                            NewMessage.BeginInvoke(this, new EventArgsMessage("Overwrite BlockChain from sender"), null, null);
                            MainViewModel.BlockChain = chainReceived;
                            MainViewModel.BlockChain.PendingTransfers = new List<Transfer>();
                            NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain updated from server"), null, null);
                        }
                        // Send a overwrite force to the sender
                        else
                        {
                            NewMessage.BeginInvoke(this, new EventArgsMessage("BlockChain receive is same size than actual but different information"), null, null);
                            Send(Constants.BLOCKCHAIN_OVERWRITE + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                        }
                    }
                }
                else if (e.Data.StartsWith(Constants.NEED_BLOCKCHAIN))
                {
                    Send(Constants.BLOCKCHAIN + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                }

                #endregion

                #region Block Receive

                else if (e.Data.StartsWith(Constants.BLOCK))
                {
                    // Deserialize the block
                    // The Substring cut "Block"
                    var newBlock = JsonConvert.DeserializeObject<Block>(e.Data.Substring(5));
                    NewMessage.BeginInvoke(this, new EventArgsMessage("New Block received"), null, null);

                    if (!MainViewModel.BlockChain.IsValid() ||
                        newBlock.PreviousHash != MainViewModel.BlockChain.LastBlock.Hash ||
                        newBlock.Index != MainViewModel.BlockChain.LastBlock.Index + 1)
                    {
                        Send(Constants.NEED_BLOCKCHAIN);
                    }
                    else
                    {
                        MainViewModel.BlockChain.Chain.Add(newBlock);
                        foreach (var tr in newBlock.Transfers)
                        {
                            MainViewModel.BlockChain.PendingTransfers.Remove(tr);
                        }

                        if (newBlock.Index % Constants.NUMBER_OF_BLOCKS_TO_CHECK_DIFFICULTY == 0)
                        {
                            NewMessage.BeginInvoke(this, new EventArgsMessage(MainViewModel.BlockChain.CheckDifficulty()), null, null);
                        }
                    }
                }

                #endregion

                #region Transfer Receive

                // The request send a new transfer
                else if (e.Data.StartsWith(Constants.TRANSFER))
                {
                    // Deserialize the transfer
                    // The Substring cut "Transfer"
                    var newTransfer = JsonConvert.DeserializeObject<Transfer>(e.Data.Substring(8));
                    NewMessage.BeginInvoke(this, new EventArgsMessage("New transfer received"), null, null);

                    // If we already have it or it's not a valid transfer don't add it
                    if (MainViewModel.BlockChain.PendingTransfers.Any(t => t.Equals(newTransfer)) || !newTransfer.IsValid())
                    {
                        NewMessage.BeginInvoke(this, new EventArgsMessage("New Transfer not valid or already in local"), null, null);
                    }
                    else
                    {
                        // If already is Ok add it to our pending transfer list
                        MainViewModel.BlockChain.PendingTransfers.Add(newTransfer);
                        NewMessage.BeginInvoke(this, new EventArgsMessage("New transfer added from server"), null, null);
                    }
                }
                // The request send a list of transfer
                else if (e.Data.StartsWith(Constants.TRANSFERS))
                {
                    // Deserialize the transfer
                    // The Substring cut "Transfer"
                    var newTransfers = JsonConvert.DeserializeObject<List<Transfer>>(e.Data.Substring(9));
                    NewMessage.BeginInvoke(this, new EventArgsMessage("New list of transfer received"), null, null);

                    foreach (var newTransfer in newTransfers)
                    {
                        // If we already have it or it's not a valid transfer don't add it
                        if (MainViewModel.BlockChain.PendingTransfers.Any(t => t.Equals(newTransfer)) || !newTransfer.IsValid())
                        {
                            NewMessage.BeginInvoke(this, new EventArgsMessage("New Transfer not valid or already in local"), null, null);
                        }
                        else
                        {
                            // If already is Ok add it to our pending transfer list
                            MainViewModel.BlockChain.PendingTransfers.Add(newTransfer);
                            NewMessage.BeginInvoke(this, new EventArgsMessage("New transfer added from server"), null, null);
                        }
                    }
                }

                #endregion

                #region GetServers Receive

                else if (e.Data.StartsWith(Constants.GET_SERVERS))
                {
                    NewMessage.BeginInvoke(this, new EventArgsMessage("Get Servers request received"), null, null);
                    var listWs = JsonConvert.DeserializeObject<List<string>>(e.Data.Substring(10));

                    ServerUpdate.BeginInvoke(this, new EventArgsObject(listWs), null, null);
                    if (MainViewModel.ServerList.Any())
                    {
                        Send("ServerList" + JsonConvert.SerializeObject(MainViewModel.ServerList.Select(x => x.Key)));
                    }
                }

                #endregion

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
        }

        #endregion
    }
}
