using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using KittyCoins.Models;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace BlockchainDemo
{
    public class P2PServer: WebSocketBehavior
    {
        bool chainSynched = false;
        WebSocketServer wss = null;

        public void Start()
        {
            wss = new WebSocketServer($"ws://127.0.0.1:{Program.Port}");
            wss.AddWebSocketService<P2PServer>("/Blockchain");
            wss.Start();
            Console.WriteLine($"Started server at ws://127.0.0.1:{Program.Port}");
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
                KittyChain newChain = JsonConvert.DeserializeObject<KittyChain>(e.Data);

                if (newChain.IsValid() && newChain.KittyChain.Count > Program.PhillyCoin.KittyChain.Count)
                {
                    List<Transfer> newTransactions = new List<Transfer>();
                    newTransactions.AddRange(newChain.PendingTransfers);
                    newTransactions.AddRange(Program.PhillyCoin.PendingTransfers);

                    newChain.PendingTransfers = newTransactions;
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
