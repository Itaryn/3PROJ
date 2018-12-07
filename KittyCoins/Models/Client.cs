using System;
using System.Collections.Generic;
using System.Linq;
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
            wsDict = new Dictionary<string, WebSocket>();
        }

        public void Connect(string url)
        {
            if (wsDict.ContainsKey(url)) return;

            var ws = new WebSocket(url);
            ws.OnMessage += (sender, e) =>
            {
                var chainReceived = JsonConvert.DeserializeObject<KittyChain>(e.Data);
                if (!chainReceived.IsValid() && !MainViewModel.BlockChain.IsValid()) return;
                if (!chainReceived.IsValid() && MainViewModel.BlockChain.IsValid())
                    Send(ws.Origin, JsonConvert.SerializeObject(MainViewModel.BlockChain));
                else if (chainReceived.Chain.Count > MainViewModel.BlockChain.Chain.Count)
                {
                    var newTransactions = new List<Transfer>();
                    newTransactions.AddRange(chainReceived.PendingTransfers);
                    newTransactions.AddRange(MainViewModel.BlockChain.PendingTransfers);

                    chainReceived.PendingTransfers = newTransactions;
                    MainViewModel.BlockChain = chainReceived;
                }
                else if (chainReceived.Chain.Count < MainViewModel.BlockChain.Chain.Count)
                    Send(ws.Origin, JsonConvert.SerializeObject(MainViewModel.BlockChain));
                else if (chainReceived.Equals(MainViewModel.BlockChain)) return;
                else
                {
                    // Si elles sont égales en tailles mais avec des blocs différents, faire un choix ou stocké les 2, etc
                }

            };
            ws.Connect();
            ws.Send(JsonConvert.SerializeObject(MainViewModel.BlockChain));
            wsDict.Add(url, ws);
        }

        public void Send(string url, string data)
        {
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