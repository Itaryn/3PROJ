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
        IDictionary<string, WebSocket> wsDict;

        public Client()
        {
            MainViewModel.MessageFromClientOrServer.Add("Create Client from client");
            wsDict = new Dictionary<string, WebSocket>();
        }

        public void Connect(string url)
        {
            MainViewModel.MessageFromClientOrServer.Add("Begin Connect function");
            
            if (wsDict.ContainsKey(url)) return;

            var ws = new WebSocket(url);
            ws.OnMessage += (sender, e) =>
            {
                try
                {
                    if (e.Data.StartsWith("BlockChain"))
                    {
                        var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data.Substring(10));
                        MainViewModel.MessageFromClientOrServer.Add("Check blockchain");
                        if (!chainReceived.IsValid() && !MainViewModel.BlockChain.IsValid() || chainReceived.Equals(MainViewModel.BlockChain)) return;
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
                            MainViewModel.MessageFromClientOrServer.Add("Blockchain receive is same size than actual but different information");
                            // Si elles sont égales en tailles mais avec des blocs différents, faire un choix ou stocké les 2, etc
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
                }
                catch (Exception)
                {
                    MainViewModel.MessageFromClientOrServer.Add("Impossible to deserialize received object");
                }
            };
            ws.Connect();
            ws.Send("BlockChain" + JsonConvert.SerializeObject(MainViewModel.BlockChain));
            wsDict.Add(url, ws);
        }

        public void Send(string url, string data)
        {
            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(data)) return;
            if (wsDict.ContainsKey(url))
            {
                wsDict[url].Send(data);
            }
        }

        public void Broadcast(string data)
        {
            foreach (var item in wsDict)
            {
                item.Value.Send(data);
            }
        }

        public IList<string> GetServers()
        {
            return wsDict.Select(item => item.Key).ToList();
        }

        public void Close()
        {
            foreach (var item in wsDict)
            {
                item.Value.Close();
            }
        }
    }
}