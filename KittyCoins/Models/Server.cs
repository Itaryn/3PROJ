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
            MainViewModel.MessageFromClientOrServer.Add($"Started server at ws://127.0.0.1:{port}");
        }

        protected override void OnMessage(MessageEventArgs e)
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
        }
    }
}
