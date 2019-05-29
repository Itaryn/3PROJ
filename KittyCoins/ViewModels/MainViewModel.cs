using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using KittyCoins.Models;
using KittyCoins.Views;
using Newtonsoft.Json;
using Prism.Commands;
using WebSocketSharp;

namespace KittyCoins.ViewModels
{
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

        public static EventHandler BlockChainUpdated;
        public static EventHandler ServerListUpdated;

        public static List<Guid> BlockChainWaitingList { get; set; }

        /// <summary>
        /// The block in creation
        /// </summary>
        private Block CurrentMineBlock { get; set; }
        public static IDictionary<string, WebSocket> ServerList;
        public Client Client;
        public Server Server;
        public Thread MiningThread;
        public Thread SaveThread;
        public User ActualUser;

        #endregion
        

        public MainViewModel()
        {
            #region Set the Commands

            RegisterCommand = new DelegateCommand(RegisterMethod);

            #endregion

            // Default Values
            Port = 6002;
            PeerUrl = "127.0.0.1:6002";
            BlockChainWaitingList = new List<Guid>();

            Client = new Client();
            Client.NewMessage += NewMessage;

            try
            {
                BlockChain = JsonConvert.DeserializeObject<KittyChain>(File.ReadAllText(Constants.SAVE_FILENAME));
            }
            catch (Exception)
            {
                BlockChain.InitializeChain();
            }
        }

        #region ICommand

        public ICommand RegisterCommand { get; }

        #endregion
        
        #region Mining

        public void Save()
        {
            while (true)
            {
                BlockChain.SaveBlockChain();
                Thread.Sleep(Constants.SCHEDULE_SAVE_TIME * 1000);
            }
        }

        public void Mining()
        {
            CurrentMineBlock = new Block(0, ActualUser.PublicAddress, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers, BlockChain.Difficulty);

            while (true)
            {
                var guid = Guid.NewGuid();
                WaitingForBlockchainAccess(guid);
                if (CurrentMineBlock.TryHash(BlockChain.Difficulty))
                {

                    Console = $"You have mined one block ! You successfull win {BlockChain.Biscuit} coins.";
                    var dif = BlockChain.LastBlock.CreationDate - DateTime.UtcNow;
                    Console = $"The last block was mined {dif:hh}h {dif:mm}m {dif:ss}s ago.";
                    Console = BlockChain.AddBlock(ActualUser.PublicAddress, CurrentMineBlock);

                    if (!BlockChain.IsValid())
                    {
                        Client.NeedBlockchain();
                        BlockChainWaitingList.Remove(guid);
                        continue;
                    }

                    Client.NewBlock(CurrentMineBlock);

                    var transfer = new Transfer(new User(Constants.PRIVATE_WORDS_KITTYCHAIN), ActualUser.PublicAddress, BlockChain.Biscuit, 0);

                    BlockChain.CreateTransfer(transfer);
                    Client.NewTransfer(transfer);
                    CurrentMineBlock = new Block(0, ActualUser.PublicAddress, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers, BlockChain.Difficulty);
                }
                BlockChainWaitingList.Remove(guid);
            }
        }

        #endregion

        #region Command Method

        public void NewTransactionMethod(object sender, EventArgs e)
        {
            if (e is EventArgsObject args)
            {
                if (args.Object is List<string> argument)
                {
                    try
                    {
                        var amount = int.Parse(argument[0]);
                        var publicAddress = argument[1];
                        var transfer = new Transfer(ActualUser, publicAddress, amount, 1);
                        Console = BlockChain.CreateTransfer(transfer);
                    }
                    catch (Exception ex)
                    {
                        Console = "Error while trying to send a new transfer";
                        Console = ex.Message;
                    }
                }
            }
        }

        public void ConnectBlockchainMethod(object sender, EventArgs e)
        {
            if (e is EventArgsMessage args)
            {
                try
                {
                    Client.Connect($"ws://{args.Message}/Blockchain");
                }
                catch (Exception ex)
                {
                    Console = "Error while trying to connect to the blockchain";
                    Console = ex.Message;
                }
            }
        }

        public void ConnectUserMethod(object sender, EventArgs e)
        {
            if (e is EventArgsObject args)
            {
                if (args.Object is User user)
                {
                    ActualUser = user;
                    Console = $"You're connected with the public address :\n{ActualUser.PublicAddress}";
                }
            }
        }

        public void LaunchServerMethod(object sender, EventArgs e)
        {
            if (e is EventArgsMessage args)
            {
                try
                {
                    var port = int.Parse(args.Message);
                    Server = new Server();
                    Console = "Create Server";
                    Client.ServerAddress = Server.Start(port);
                    Console = $"Open server at {Client.ServerAddress}";
                    Console = $"Start server with port n°{port}";

                    Server.NewMessage += NewMessage;
                    Server.ServerUpdate += UpdateServer;

                    SaveThread = new Thread(Save) { IsBackground = true };
                    SaveThread.Start();
                }
                catch (Exception ex)
                {
                    Console = "Error when trying to launch the server";
                    Console = ex.Message;

                    Server?.wss.Stop();
                    SaveThread?.Abort();
                }
            }
        }

        public void RegisterMethod()
        {
            var registerView = new RegisterView();
            registerView.Show();
        }

        public static void WaitingForBlockchainAccess(Guid guid)
        {
            var waitingTime = 0;
            var first = BlockChainWaitingList.FirstOrDefault();
            BlockChainWaitingList.Add(guid);

            bool result;
            try
            {
                result = !BlockChainWaitingList.FirstOrDefault().Equals(guid);
            }
            catch (Exception)
            {
                result = true;
            }

            while (result)
            {
                if (waitingTime > Constants.WAITING_TIME_MAX * 2000)
                {
                    if (first != null &&
                        first == BlockChainWaitingList.FirstOrDefault())
                    {
                        BlockChainWaitingList.Remove(first);
                    }
                    else
                    {
                        first = BlockChainWaitingList.FirstOrDefault(); ;
                        waitingTime = 0;
                    }
                }
                waitingTime++;
                Thread.Sleep(5);
                try
                {
                    result = !BlockChainWaitingList.FirstOrDefault().Equals(guid);
                }
                catch (Exception)
                {
                    result = true;
                }
            }

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