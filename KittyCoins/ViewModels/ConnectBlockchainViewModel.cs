using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using KittyCoins.Models;
using Prism.Commands;

namespace KittyCoins.ViewModels
{
    public class ConnectBlockchainViewModel : INotifyPropertyChanged
    {
        private string _serverAddress;

        public EventHandler LaunchServerWithPort;

        public ConnectBlockchainViewModel()
        {
            ConnectToBlockchainCommand = new DelegateCommand(ConnectToBlockchainMethod);
        }

        public ICommand ConnectToBlockchainCommand { get; }

        public void ConnectToBlockchainMethod()
        {
            LaunchServerWithPort.BeginInvoke(this, new EventArgsMessage(ServerAddress), null, null);
        }

        #region Input

        public string ServerAddress
        {
            get => _serverAddress;
            set
            {
                if (_serverAddress == value) return;
                _serverAddress = value;
                RaisePropertyChanged("ServerAddress");
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