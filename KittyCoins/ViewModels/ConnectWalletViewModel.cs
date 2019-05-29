using System;
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

        public EventHandler ConnectWithWords;

        public ConnectWalletViewModel(string publicAddress)
        {
            ConnectWithWordsCommand = new DelegateCommand(ConnectWithWordsMethod);
            ConnectWithFileCommand = new DelegateCommand(ConnectWithFileMethod);

            PublicAddress = publicAddress;

            MainViewModel.BlockChainUpdated += UpdateUserBalance;

            var receivers = MainViewModel.BlockChainUpdated?.GetInvocationList();
            if (receivers != null)
            {
                foreach (EventHandler receiver in receivers)
                {
                    receiver.BeginInvoke(this, EventArgs.Empty, null, null);
                }
            }
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
        }

        private void UpdateUserBalance(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(PublicAddress))
            {
                Balance = MainViewModel.BlockChain.GetBalance(PublicAddress);
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