using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Windows.Input;
using KittyCoin.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using Prism.Commands;

namespace KittyCoin.ViewModels
{
    public class ConnectWalletViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The private words
        /// </summary>
        private string _privateWords;

        /// <summary>
        /// The public address
        /// </summary>
        private string _publicAddress;

        /// <summary>
        /// The balance
        /// </summary>
        private double _balance;

        /// <summary>
        /// The transaction list
        /// </summary>
        private Dictionary<Transfer, bool> _transferHistory;

        /// <summary>
        /// The information message about the wallet
        /// </summary>
        private string _walletConnectMessage;

        /// <summary>
        /// Event Handler, invoke when the user is trying to connect is wallet
        /// </summary>
        /// <remarks>
        /// It's invoke even if the user connect with a file
        /// </remarks>
        public EventHandler ConnectWithWords;

        /// <summary>
        /// Event Handler, invoke when the user is connected
        /// </summary>
        public EventHandler UserChanged;

        /// <summary>
        /// Create the ViewModel with the public address of the user
        /// </summary>
        /// <param name="publicAddress"></param>
        public ConnectWalletViewModel(string publicAddress)
        {
            ConnectWithWordsCommand = new DelegateCommand(ConnectWithWordsMethod);
            ConnectWithFileCommand = new DelegateCommand(ConnectWithFileMethod);
            SaveItInFileCommand = new DelegateCommand(SaveItInFileMethod);

            PublicAddress = publicAddress;
            UpdateUserBalance(null, null);

            MainViewModel.BlockChainUpdated += UpdateUserBalance;
        }

        /// <summary>
        /// The ICommand for the button "Connect with words"
        /// </summary>
        public ICommand ConnectWithWordsCommand { get; }

        /// <summary>
        /// The ICommand for the button "Connect with file"
        /// </summary>
        public ICommand ConnectWithFileCommand { get; }
        
        /// <summary>
        /// The ICommand for the button "Save it in file"
        /// </summary>
        public ICommand SaveItInFileCommand { get; }

        /// <summary>
        /// The method use when the user click the button "Connect with words"
        /// It create the user from the words list
        /// </summary>
        /// <see cref="User"/>
        public void ConnectWithWordsMethod()
        {
            UpdateUser(new User(PrivateWords));
        }

        /// <summary>
        /// The method use when the user click the button "Connect with file"
        /// It create the user from the selected file
        /// </summary>
        /// <see cref="User"/>
        public void ConnectWithFileMethod()
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var privateKey = JsonConvert.DeserializeObject<RSAParameters>(File.ReadAllText(openFileDialog.FileName));
                    UpdateUser(new User(privateKey));
                }
                catch (Exception)
                {
                    WalletConnectMessage = "Error with your file";
                }

            }
        }

        /// <summary>
        /// Save the private key to a file choose by the user
        /// </summary>
        public void SaveItInFileMethod()
        {
            var saveFileDialog = new SaveFileDialog();

            if (saveFileDialog.ShowDialog() == true)
            {
                var user = new User(PrivateWords);
                user.SaveToFile(saveFileDialog.FileName);
            }
        }

        /// <summary>
        /// This method is called by the 2 methods "ConnectWith..."
        /// It send the user to the MainViewModel from the EventHandler "ConnectWithWords"
        /// </summary>
        /// <param name="user"></param>
        /// <see cref="MainViewModel.ConnectUserMethod"/>
        /// <seealso cref="UpdateUserBalance"/>
        public void UpdateUser(User user)
        {
            PublicAddress = user.PublicAddress;
            UpdateUserBalance(this, EventArgs.Empty);
            ConnectWithWords.BeginInvoke(this, new EventArgsObject(user), null, null);

            UserChanged?.Invoke(null, null);
        }

        /// <summary>
        /// Update the balance and transaction list from the public address
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateUserBalance(object sender, EventArgs e)
        {
            WalletConnectMessage = "You don't have a connected wallet";
            if (!string.IsNullOrEmpty(PublicAddress))
            {
                Balance = MainViewModel.BlockChain.GetBalance(PublicAddress);
                var transactions = MainViewModel.BlockChain.GetTransactions(PublicAddress);

                var transactionDic = new Dictionary<Transfer, bool>();
                foreach (var transaction in transactions)
                {
                    transactionDic.Add(transaction, transaction.FromAddress.Equals(PublicAddress));
                }

                TransactionHistory = transactionDic;

                WalletConnectMessage = Constants.WALLET_CONNECTED;
            }
        }

        #region Input

        /// <summary>
        /// The input string private words
        /// </summary>
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

        /// <summary>
        /// The string PublicAddress
        /// </summary>
        public string PublicAddress
        {
            get => _publicAddress;
            set
            {
                if (_publicAddress == value) return;
                _publicAddress = value;
                RaisePropertyChanged("PublicAddress");
            }
        }

        /// <summary>
        /// The double Balance
        /// </summary>
        public double Balance
        {
            get => _balance;
            set
            {
                if (_balance == value) return;
                _balance = value;
                RaisePropertyChanged("Balance");
            }
        }

        /// <summary>
        /// The list of transaction
        /// </summary>
        /// <remarks>
        /// Each line have a boolean to know if the user win or lose coins
        /// </remarks>
        public Dictionary<Transfer, bool> TransactionHistory
        {
            get => _transferHistory;
            set
            {
                if (_transferHistory == value) return;
                _transferHistory = value;
                RaisePropertyChanged("TransactionHistory");
            }
        }

        /// <summary>
        /// The message about wallet statue
        /// </summary>
        public string WalletConnectMessage
        {
            get => _walletConnectMessage;
            set
            {
                if (_walletConnectMessage == value) return;
                _walletConnectMessage = value;
                RaisePropertyChanged("WalletConnectMessage");
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