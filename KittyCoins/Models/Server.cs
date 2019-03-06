using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KittyCoins.ViewModels;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KittyCoins.Models
{
    public class Server : WebSocketBehavior
    {
        public WebSocketServer wss;
        public static string serverAddress = "";
        
        public void Start(int port)
        {
            wss = new WebSocketServer($"ws://127.0.0.1:{port}");
            wss.AddWebSocketService<Server>("/Blockchain");
            wss.Start();

            serverAddress = $"ws://127.0.0.1:{port}/Blockchain";
            MainViewModel.MessageFromClientOrServer.Add($"Started server at {serverAddress}");

        }

        protected override void OnMessage(MessageEventArgs e)
        {
            try
            {
                if (e.Data.StartsWith("BlockChain"))
                {
                    var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(e.Data.StartsWith("BlockChainOverwrite") ? 19 : 10));
                    MainViewModel.MessageFromClientOrServer.Add("Check blockchain");
                    if (!chainReceived.IsValid() && !MainViewModel.BlockChain.IsValid() ||
                        chainReceived.Equals(MainViewModel.BlockChain) ||
                        chainReceived.Chain.Equals(MainViewModel.BlockChain.Chain) &&
                        chainReceived.PendingTransfers.Equals(MainViewModel.BlockChain.PendingTransfers)) return;
                    if (!chainReceived.IsValid() && MainViewModel.BlockChain.IsValid())
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Blockchain receive not valid but actual is");
                        Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }
                    else if (chainReceived.Chain.Count > MainViewModel.BlockChain.Chain.Count)
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Blockchain is bigger than actual");
                        var newTransactions = new List<Transfer>();
                        newTransactions.AddRange(chainReceived.PendingTransfers);
                        newTransactions.AddRange(MainViewModel.BlockChain.PendingTransfers);

                        chainReceived.PendingTransfers = newTransactions;
                        MainViewModel.BlockChain = chainReceived;
                    }
                    else if (chainReceived.Chain.Count < MainViewModel.BlockChain.Chain.Count)
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Blockchain receive lower than actual");
                        Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }
                    else if (chainReceived.Chain.Equals(MainViewModel.BlockChain.Chain) &&
                             !chainReceived.PendingTransfers.Equals(MainViewModel.BlockChain.PendingTransfers))
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Chain equals but different pending transfers");
                        MainViewModel.BlockChain.PendingTransfers.AddRange(chainReceived.PendingTransfers.Except(MainViewModel.BlockChain.PendingTransfers));
                        Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                    }
                    else
                    {
                        if (e.Data.StartsWith("BlockChainOverwrite"))
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Overwrite BlockChain from sender");
                            MainViewModel.BlockChain = chainReceived;
                        }
                        else
                        {
                            MainViewModel.MessageFromClientOrServer.Add("BlockChain receive is same size than actual but different information");
                            Send("BlockChainOverwrite" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                        }
                    }
                }
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
                    
                    if (listWs.Except(MainViewModel.wsDict.Keys).Any())
                    {
                        MainViewModel.MessageFromClientOrServer.Add("Connect to the others servers");

                        foreach (var address in listWs.Except(MainViewModel.wsDict.Keys))
                            MainViewModel.Client.Connect(address);
                    }

                    if (MainViewModel.wsDict.Keys.Except(listWs).Any())
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
    }
}
