using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using KittyCoins.Models;
using Microsoft.Win32;
using Prism.Commands;

namespace KittyCoins.ViewModels
{
    public class ConnectWalletViewModel : INotifyPropertyChanged
    {
        private string _privateWords;
        private string _publicAddress;
        private double _balance;
        private Dictionary<Transfer, bool> _transferHistory;
        private string _walletConnectMessage;

        public EventHandler ConnectWithWords;

        public EventHandler UserChanged;

        public ConnectWalletViewModel(string publicAddress)
        {
            ConnectWithWordsCommand = new DelegateCommand(ConnectWithWordsMethod);
            ConnectWithFileCommand = new DelegateCommand(ConnectWithFileMethod);

            PublicAddress = publicAddress;
            UpdateUserBalance(null, null);

            MainViewModel.BlockChainUpdated += UpdateUserBalance;
        }

        public ICommand ConnectWithWordsCommand { get; }
        public ICommand ConnectWithFileCommand { get; }

        public void ConnectWithWordsMethod()
        {
            UpdateUser(PrivateWords);
        }

        public void ConnectWithFileMethod()
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                UpdateUser(File.ReadAllText(openFileDialog.FileName));
            }
        }

        public void UpdateUser(string privateWords)
        {
            var user = new User(privateWords);
            PublicAddress = user.PublicAddress;
            UpdateUserBalance(this, EventArgs.Empty);
            ConnectWithWords.BeginInvoke(this, new EventArgsObject(user), null, null);

            UserChanged?.Invoke(null, null);
        }

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