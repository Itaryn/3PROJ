using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KittyCoins.ViewModels;
using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;
/*
namespace KittyCoins.Models
{
    public class Server : WebSocketBehavior
    {
        bool chainSynched = false;
        WebSocketServer wss = null;

        public void Start(int port)
        {
            wss = new WebSocketServer($"ws://127.0.0.1:{port}");
            wss.AddWebSocketService<Server>("/Blockchain");
            wss.Start();
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            if (e.Data == "Hi Server")
            {
                Console.WriteLine(e.Data);
                Send("Hi Client");
            }
            else
            {
                Blockchain newChain = JsonConvert.DeserializeObject<Blockchain>(e.Data);

                if (newChain.IsValid() && newChain.Chain.Count > Program.PhillyCoin.Chain.Count)
                {
                    List<Transaction> newTransactions = new List<Transaction>();
                    newTransactions.AddRange(newChain.PendingTransactions);
                    newTransactions.AddRange(Program.PhillyCoin.PendingTransactions);

                    newChain.PendingTransactions = newTransactions;
                    Program.PhillyCoin = newChain;
                }

                if (!chainSynched)
                {
                    Send(JsonConvert.SerializeObject(Program.PhillyCoin));
                    chainSynched = true;
                }
            }
        }
    }
}
*/
