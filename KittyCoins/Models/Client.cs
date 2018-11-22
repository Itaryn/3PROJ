using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KittyCoins.Models
{
    public class Client : WebSocketBehavior
    {
        IDictionary<string, WebSocket> wsDict;
        public Blockchain Chain;
        WebSocketServer wss;
        
        public Client(int port)
        {
            wsDict = new Dictionary<string, WebSocket>();
            Chain = new Blockchain();

            wss = new WebSocketServer($"ws://127.0.0.1:{port}");
            wss.Start();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var chainReceived = JsonConvert.DeserializeObject<Blockchain>(e.Data);
            if (!chainReceived.IsValid() && !Chain.IsValid()) return;
            if (!chainReceived.IsValid() && Chain.IsValid())
                Send(JsonConvert.SerializeObject(Chain));
            else if (chainReceived.KittyChain.Count > Chain.KittyChain.Count)
            {
                var newTransactions = new List<Transfer>();
                newTransactions.AddRange(chainReceived.PendingTransfers);
                newTransactions.AddRange(Chain.PendingTransfers);

                chainReceived.PendingTransfers = newTransactions;
                Chain = chainReceived;
            }
            else if (chainReceived.KittyChain.Count < Chain.KittyChain.Count)
                Send(JsonConvert.SerializeObject(Chain));
            else if (chainReceived.Equals(Chain)) return;
            else
            {
                // Si elles sont égales en tailles mais avec des blocs différents, faire un choix ou stocké les 2, etc
            }
        }

        public void SendTo(string url, string data)
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