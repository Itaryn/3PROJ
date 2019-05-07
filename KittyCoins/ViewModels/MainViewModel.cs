using System;
using System.Security.Cryptography;
using KittyCoins.Packages;

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
        private string _privateWords;
        private string _consoleOutput = "";

        #endregion

        #region Public Attributes

        public static KittyChain BlockChain = new KittyChain();

        public static bool BlockChainAccessToken { get; set; }

        public static List<Guid> BlockChainWaitingList { get; set; }

        /// <summary>
        /// The block in creation
        /// </summary>
        private Block CurrentMineBlock { get; set; }
        public static IDictionary<string, WebSocket> ServerList;
        public Client Client;
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
            ConnectBlockchainCommand = new DelegateCommand(ConnectBlockchainMethod);
            ConnectUserCommand = new DelegateCommand(ConnectUserMethod);
            NewTransactionCommand = new DelegateCommand(NewTransactionMethod);
            RegisterCommand = new DelegateCommand(RegisterMethod);

            #endregion

            // Default Values
            Port = 6002;
            PeerUrl = "127.0.0.1:6002";
            PrivateWords = "Guilhem";
            ActualUser = new User(PrivateWords);
            BlockChainAccessToken = true;
            BlockChainWaitingList = new List<Guid>();

            Client = new Client();
            Client.NewMessage += NewMessage;

            try
            {
                var blockSaved = Directory.GetFiles(Constants.DATABASE_FOLDER);
                if (blockSaved.Any())
                {
                    var blocks = blockSaved.Select(JsonConvert.DeserializeObject<Block>).ToList();
                    BlockChain = new KittyChain(blocks, new List<Transfer>());
                }
            }
            catch (Exception)
            {
                BlockChain.InitializeChain();
            }
        }

        #region ICommand

        public ICommand LaunchServerCommand { get; }
        public ICommand ShowBlockChainCommand { get; }
        public ICommand ConnectBlockchainCommand { get; }
        public ICommand ConnectUserCommand { get; }
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
            Client.ServerAddress = Server.Start(Port);
            Console = $"Open server at {Client.ServerAddress}";
            Console = $"Start server with port n°{Port}";

            Server.NewMessage += NewMessage;
            Server.ServerUpdate += UpdateServer;
        }

        public void Mining()
        {
            CurrentMineBlock = new Block(0, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers, BlockChain.Difficulty);

            while (true)
            {
                if (CurrentMineBlock.TryHash(BlockChain.Difficulty))
                {
                    Console = $"You have mined one block ! You successfull win {BlockChain.Biscuit} coins.";
                    var dif = BlockChain.Chain.Last().CreationDate - DateTime.UtcNow;
                    Console = $"The last block was mined {dif:hh}h {dif:mm}m {dif:ss}s ago.";
                    Console = BlockChain.AddBlock(ActualUser.PublicAddress, CurrentMineBlock);

                    Client.NewBlock(CurrentMineBlock);

                    var transfer = new Transfer(new User(Constants.PRIVATE_WORDS_KITTYCHAIN), ActualUser.PublicAddress, BlockChain.Biscuit, 0);

                    BlockChain.CreateTransfer(transfer);
                    Client.NewTransfer(transfer);
                    CurrentMineBlock = new Block(0, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers, BlockChain.Difficulty);
                }
            }
        }

        #endregion

        #region Command Method

        public void ShowBlockChainMethod()
        {
            var blockChainView = new BlockChainView(BlockChain);
            blockChainView.Show();
        }

        public void NewTransactionMethod()
        {
            var transfer = new Transfer(new User("Guilhem"), new User("Loic").PublicAddress, 15, 1);
            Console = BlockChain.CreateTransfer(transfer);
        }

        public void ConnectBlockchainMethod()
        {
            Client.Connect($"ws://{PeerUrl}/Blockchain");
        }

        public void ConnectUserMethod()
        {
            ActualUser = new User(PrivateWords);
        }

        public void RegisterMethod()
        {
            var registerView = new RegisterView();
            registerView.Show();
        }

        #endregion

        #region Private Methods

        private void NewMessage(object sender, EventArgs e)
        {
            if (e is EventArgsMessage args)
            {
                Console = args.Message;
            }
            else if (e is EventArgsObject argObj &&
                     argObj.Object is Transfer transfer)
            {
                Client.NewTransfer(transfer);
            }
        }

        private void UpdateServer(object sender, EventArgs e)
        {
            if (sender is Server &&
                e is EventArgsObject args &&
                args.Object is List<string> servers)
            {
                Client.ConnectToAll(servers);
            }
        }

        #endregion

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
        public string PrivateWords
        {
            get => _privateWords;
            set
            {
                if (_privateWords == value) return;
                _privateWords = value;
                RaisePropertyChanged("PrivateWords");
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