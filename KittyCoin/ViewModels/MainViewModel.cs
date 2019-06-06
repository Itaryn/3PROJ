using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using KittyCoin.Models;
using KittyCoin.Views;
using Newtonsoft.Json;
using Prism.Commands;
using WebSocketSharp;

namespace KittyCoin.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Private Attributes

        /// <summary>
        /// Boolean who represent if the user mine or not
        /// </summary>
        private bool _checkBoxMine;

        /// <summary>
        /// The string in the console
        /// </summary>
        private string _consoleOutput = "";

        #endregion

        #region Public Attributes

        /// <summary>
        /// The Blockchain
        /// </summary>
        /// <remarks>
        /// It's initialize when the app is launched
        /// </remarks>
        /// <see cref="KittyChain"/>
        public static KittyChain BlockChain = new KittyChain();

        /// <summary>
        /// The EventHandler used when the blockchain is updated
        /// </summary>
        public static EventHandler BlockChainUpdated;

        /// <summary>
        /// The EventHandler used when the server list is updated
        /// </summary>
        public static EventHandler ServerListUpdated;

        /// <summary>
        /// List of Guid to schedule access for the blockchain
        /// </summary>
        /// <see cref="Guid"/>
        public static List<Guid> BlockChainWaitingList { get; set; }

        /// <summary>
        /// The block in creation
        /// </summary>
        private Block CurrentMineBlock { get; set; }

        /// <summary>
        /// The list of connected server
        /// </summary>
        public static IDictionary<string, WebSocket> ServerList;

        /// <summary>
        /// The client
        /// </summary>
        /// <see cref="Client"/>
        public Client Client;

        /// <summary>
        /// The server
        /// </summary>
        /// <see cref="Server"/>
        public Server Server;

        /// <summary>
        /// The mining thread
        /// </summary>
        /// <see cref="Thread"/>
        public Thread MiningThread;

        /// <summary>
        /// The blockchain save thread
        /// </summary>
        /// <see cref="Thread"/>
        public Thread SaveThread;

        /// <summary>
        /// The wallet connected
        /// </summary>
        /// <see cref="User"/>
        public User ActualUser;

        #endregion
        

        public MainViewModel()
        {
            #region Set the Commands

            RegisterCommand = new DelegateCommand(RegisterMethod);

            #endregion

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

        /// <summary>
        /// Method who save the blockchain in a txt file, the interval is set in Constants
        /// </summary>
        /// <see cref="Constants.SCHEDULE_SAVE_TIME"/>
        public void Save()
        {
            while (true)
            {
                BlockChain.SaveBlockChain();
                Thread.Sleep(Constants.SCHEDULE_SAVE_TIME * 1000);
            }
        }

        /// <summary>
        /// Method to try hash for a new block
        /// </summary>
        /// <see cref="Block.TryHash"/>
        public void Mining()
        {
            CurrentMineBlock = new Block(0, ActualUser.PublicAddress, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers, BlockChain.Difficulty);

            while (true)
            {
                var guid = Guid.NewGuid();
                WaitingForBlockchainAccess(guid);

                var difficulty = BlockChain.Difficulty;
                var previousHash = BlockChain.LastBlock.Hash;
                var transfers = BlockChain.PendingTransfers.ToArray();

                BlockChainWaitingList.Remove(guid);

                if (CurrentMineBlock.TryHash(difficulty, previousHash, transfers))
                {
                    guid = Guid.NewGuid();
                    WaitingForBlockchainAccess(guid);

                    var dif = BlockChain.LastBlock.CreationDate - DateTime.UtcNow;
                    Console = BlockChain.AddBlock(CurrentMineBlock);

                    BlockChainWaitingList.Remove(guid);

                    if (!BlockChain.IsValid())
                    {
                        Client.NeedBlockchain();
                    }
                    else
                    {
                        var biscuit = CurrentMineBlock.GetBiscuit(BlockChain.Biscuit);

                        Console = $"You have mined one block ! You successfull win {biscuit} coins.";
                        Console = $"The last block was mined {dif:hh}h {dif:mm}m {dif:ss}s ago.";

                        Client.NewBlock(CurrentMineBlock);

                        var transfer = new Transfer(new User(Constants.PRIVATE_WORDS_KITTYCHAIN), ActualUser.PublicAddress, biscuit, 0);

                        BlockChain.CreateTransfer(transfer);
                        Client.NewTransfer(transfer);
                        CurrentMineBlock = new Block(0, ActualUser.PublicAddress, BlockChain.Chain.Last().Hash, BlockChain.PendingTransfers, BlockChain.Difficulty);

                        var receivers = MainViewModel.BlockChainUpdated?.GetInvocationList();
                        if (receivers != null)
                        {
                            foreach (EventHandler receiver in receivers)
                            {
                                receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Command Method

        /// <summary>
        /// Add a new transaction to the blockchain and broadcast it to the connected server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="NewTransactionViewModel.SendTransactionMethod"/>
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
                        var message = BlockChain.CreateTransfer(transfer);

                        if (message == Constants.TRANSFER_ADDED)
                        {
                            Client.NewTransfer(transfer);
                        }

                        Console = message;
                    }
                    catch (Exception ex)
                    {
                        Console = "Error while trying to send a new transfer";
                        Console = ex.Message;
                    }
                }
            }
        }

        /// <summary>
        /// Connect the user to a server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="ConnectBlockchainViewModel.ConnectToBlockchainMethod"/>
        public void ConnectBlockchainMethod(object sender, EventArgs e)
        {
            if (Server == null)
            {
                Console = "Launch your server with a port before connecting to the BlockChain Network";
                return;
            }
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

        /// <summary>
        /// Set the ActualUser to the new wallet connected
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="ConnectWalletViewModel.UpdateUser"/>
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

        /// <summary>
        /// Launch the server with the given port
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="LaunchServerViewModel.LaunchServerMethod"/>
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

                    Server?.wss?.Stop();
                    SaveThread?.Abort();
                }
            }
        }

        /// <summary>
        /// Show the popup to register a new wallet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="RegisterView"/>
        public void RegisterMethod()
        {
            var registerView = new RegisterView();
            registerView.Show();
        }

        /// <summary>
        /// Method who wait to access the blockchain
        /// </summary>
        /// <param name="guid"></param>
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
        public string Console
        {
            get => _consoleOutput;
            set
            {
                if (_consoleOutput == value ||
                    string.IsNullOrEmpty(value)) return;
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