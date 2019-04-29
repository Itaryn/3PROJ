using System;

namespace KittyCoins.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Windows.Input;
    using Models;
    using Views;
    using Prism.Commands;
    using WebSocketSharp;
    using System.IO;
    using Newtonsoft.Json;

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Attributes

        private bool _checkBoxMine;
        private int _port;
        private string _peerUrl;
        private string _consoleOutput = "";

        #endregion

        #region Public Attributes

        public static List<string> MessageFromClientOrServer = new List<string>();
        public static KittyChain BlockChain = new KittyChain();

        /// <summary>
        /// The block in creation
        /// </summary>
        private Block CurrentMineBlock { get; set; }
        public static IDictionary<string, WebSocket> WsDict;
        public static Client Client = new Client();
        public Server Server;
        public Thread MiningThread;
        public Thread OpenServerThread;
        public User ActualUser;

        #endregion
        

        public MainViewModel()
        {
            #region Set the Commands

            LaunchServerCommand = new DelegateCommand(LaunchServerMethod);
            ShowBlockChainCommand = new DelegateCommand(ShowBlockChainMethod);
            NewTransactionCommand = new DelegateCommand(NewTransactionMethod);
            RegisterCommand = new DelegateCommand(RegisterMethod);

            #endregion

            // Default Values
            Port = 6002;
            PeerUrl = "127.0.0.1:6002";

            BlockChain.InitializeChain();
            var blockSaved = Directory.GetFiles(Constants.DATABASE_FOLDER);
            if (blockSaved.Any())
            {
                var blocks = blockSaved.Select(JsonConvert.DeserializeObject<Block>).ToList();
                BlockChain = new KittyChain(blocks, new List<Transfer>());
            }
        }

        #region ICommand

        public ICommand LaunchServerCommand { get; }
        public ICommand ShowBlockChainCommand { get; }
        public ICommand NewTransactionCommand { get; }
        public ICommand RegisterCommand { get; }

        #endregion
        
        #region Mining

        public void LaunchServerMethod()
        {
            OpenServerThread = new Thread(OpenServer) {IsBackground = true};
            OpenServerThread.Start();
        }

        public void OpenServer()
        {
            Server = new Server();
            Console = "Create Server";
            Server.Start(Port);
            Console = $"Start server with port n°{Port}";
            
            while (true)
            {
                if (MessageFromClientOrServer != null && MessageFromClientOrServer.Count != 0)
                {
                    Console = MessageFromClientOrServer.First();
                    MessageFromClientOrServer.RemoveAt(0);
                }
                Thread.Sleep(100);
            }
        }

        public void Mining()
        {
            CurrentMineBlock = new Block(0, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers);

            while (true)
            {
                if (CurrentMineBlock.TryHash(BlockChain.Difficulty))
                {
                    Console = "You have mined one block ! You successfull win 10 coins.";
                    var dif = BlockChain.Chain.Last().CreationDate - DateTime.UtcNow;
                    if (dif.TotalSeconds < 60 * 10)
                    {
                        Console = "Difficulty up !";
                        BlockChain.Difficulty++;
                    }
                    Console = $"The last block was mined {dif:hh}h {dif:mm}m {dif:ss}s ago.";
                    BlockChain.AddBlock("", CurrentMineBlock);
                    Client.NewBlock(BlockChain);
                    CurrentMineBlock = new Block(0, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers);
                }
            }
        }

        #endregion

        public void ShowBlockChainMethod()
        {
            var blockChainView = new BlockChainView();
            blockChainView.Show();
        }

        public void NewTransactionMethod()
        {
            Client.Connect($"ws://127.0.0.1:{Port}/Blockchain");
            //var transfer = new Transfer("Guilhem", "Loic", 15, 1, new RSACryptoServiceProvider().ExportParameters(true));
            //BlockChain.CreateTransfer(transfer);
            //Client.Send("ws://127.0.0.1:6002/Blockchain", "Transfer" + JsonConvert.SerializeObject(transfer));
        }

        public void RegisterMethod()
        {
            var registerView = new RegisterView();
            registerView.Show();
        }

        #region Input
        public bool CheckBoxMine
        {
            get => _checkBoxMine;
            set
            {
                if (_checkBoxMine == value) return;
                _checkBoxMine = value;

                if (_checkBoxMine)
                {
                    Console = "Begin mining";
                    MiningThread = new Thread(Mining) { IsBackground = true };
                    MiningThread.Start();
                }
                else
                {
                    Console = "Stop mining";
                    MiningThread?.Abort();
                }

                RaisePropertyChanged("CheckBoxMine");
            }
        }
        public int Port
        {
            get => _port;
            set
            {
                if (_port == value) return;
                _port = value;
                RaisePropertyChanged("Port");
            }
        }
        public string PeerUrl
        {
            get => _peerUrl;
            set
            {
                if (_peerUrl == value) return;
                _peerUrl = value;
                RaisePropertyChanged("PeerUrl");
            }
        }
        public string Console
        {
            get => _consoleOutput;
            set
            {
                if (_consoleOutput == value) return;
                _consoleOutput += value + "\n";
                RaisePropertyChanged("Console");
            }
        }
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}