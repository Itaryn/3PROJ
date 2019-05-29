using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using KittyCoins.Models;
using Prism.Commands;

namespace KittyCoins.ViewModels
{
    public class ConnectBlockchainViewModel : INotifyPropertyChanged
    {
        private string _serverAddress;
        private List<string> _serverList;

        public EventHandler LaunchServerWithPort;

        public ConnectBlockchainViewModel(IEnumerable<string> serverList)
        {
            ConnectToBlockchainCommand = new DelegateCommand(ConnectToBlockchainMethod);

            MainViewModel.ServerListUpdated += ServerListUpdate;
            ServerList = serverList.ToList();
        }

        private void ServerListUpdate(object sender, EventArgs e)
        {
            ServerList = MainViewModel.ServerList.Keys.ToList();
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

        public List<string> ServerList
        {
            get => _serverList;
            set
            {
                if (_serverList == value) return;
                _serverList = value;
                RaisePropertyChanged("ServerList");
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