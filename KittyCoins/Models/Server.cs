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

        public void Start(int port)
        {
            wss = new WebSocketServer($"ws://127.0.0.1:{port}");
            wss.AddWebSocketService<Server>("/Blockchain");
            wss.Start();
            Console.WriteLine($"Started server at ws://127.0.0.1:{port}");
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            var chainReceived = JsonConvert.DeserializeObject<Blockchain>(e.Data);
            if (!chainReceived.IsValid() && !MainViewModel.BlockChain.IsValid()) return;
            if (!chainReceived.IsValid() && MainViewModel.BlockChain.IsValid())
                Send(JsonConvert.SerializeObject(MainViewModel.BlockChain));
            else if (chainReceived.KittyChain.Count > MainViewModel.BlockChain.KittyChain.Count)
            {
                var newTransactions = new List<Transfer>();
                newTransactions.AddRange(chainReceived.PendingTransfers);
                newTransactions.AddRange(MainViewModel.BlockChain.PendingTransfers);

                chainReceived.PendingTransfers = newTransactions;
                MainViewModel.BlockChain = chainReceived;
            }
            else if (chainReceived.KittyChain.Count < MainViewModel.BlockChain.KittyChain.Count)
                Send(JsonConvert.SerializeObject(MainViewModel.BlockChain));
            else if (chainReceived.Equals(MainViewModel.BlockChain)) return;
            else
            {
                // Si elles sont égales en tailles mais avec des blocs différents, faire un choix ou stocké les 2, etc
            }
        }
    }
}
