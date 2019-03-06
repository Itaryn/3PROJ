using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using KittyCoins.ViewModels;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KittyCoins.Models
{
    public class Client
    {
        public Client()
        {
            MainViewModel.MessageFromClientOrServer.Add("Create Client from client");
            MainViewModel.wsDict = new Dictionary<string, WebSocket>();
        }

        public void Connect(string url)
        {
            if (MainViewModel.wsDict.ContainsKey(url) || url == Server.serverAddress) return;

            MainViewModel.MessageFromClientOrServer.Add($"Begin Connection to {url}");

            var ws = new WebSocket(url);
            ws.OnMessage += (sender, e) =>
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
                            Send(ws.Origin, "BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
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
                            Send(ws.Origin, "BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
                        }
                        else if (chainReceived.Chain.Equals(MainViewModel.BlockChain.Chain) &&
                                 !chainReceived.PendingTransfers.Equals(MainViewModel.BlockChain.PendingTransfers))
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Chain equals but different pending transfers");
                            MainViewModel.BlockChain.PendingTransfers.AddRange(chainReceived.PendingTransfers.Except(MainViewModel.BlockChain.PendingTransfers));
                            Send(ws.Origin, "BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
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
                                Send(ws.Origin, "BlockChainOverwrite" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
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
                        if (!MainViewModel.wsDict.Keys.Except(listWs).Any())
                        {
                            MainViewModel.MessageFromClientOrServer.Add("Connect to the others servers");

                            foreach (var address in listWs.Except(MainViewModel.wsDict.Keys))
                                MainViewModel.Client.Connect(address);

                            Send(ws.Origin, "GetServers" + JsonConvert.SerializeObject(new List<string>(MainViewModel.wsDict.Keys) { Server.serverAddress }));
                        }
                    }
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
            MainViewModel.wsDict.Add(url, ws);
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
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(data)) return;
            if (MainViewModel.wsDict.ContainsKey(url))
                MainViewModel.wsDict[url].Send(data);
        }

        /// <summary>
        /// Send a message (data) to all server in the dictionnary (wsDict)
        /// </summary>
        /// <param name="data"></param>
        public void Broadcast(string data)
        {
            foreach (var item in MainViewModel.wsDict)
            {
                item.Value.Send(data);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>
        /// The list of server url
        /// </returns>
        public IList<string> GetServers()
        {
            return MainViewModel.wsDict.Select(item => item.Key).ToList();
        }

        /// <summary>
        /// Close all connection
        /// </summary>
        public void Close()
        {
            foreach (var item in MainViewModel.wsDict)
            {
                item.Value.Close();
            }
        }
    }
}