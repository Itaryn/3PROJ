using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using KittyCoins.Models;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace KittyCoins
{
    class Main
    {
        public int Port;
        //public static P2PServer Server = null;
        //public static P2PClient Client = new P2PClient();
        public static Blockchain KittyChain = new Blockchain();
        public string Name = "Unknown";

        public Main(int port, string name)
        {
            KittyChain.InitializeChain();

            Port = port;
            Name = name;

            if (Port > 0)
            {
               // Server = new P2PServer();
               // Server.Start();
            }

            while (true)
            {
            }

            //Client.Close();
        }
    }
}