using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using KittyCoin.Models;
using Prism.Commands;

namespace KittyCoin.ViewModels
{
    public class ConnectBlockchainViewModel : INotifyPropertyChanged
    {
        /// <summary>
        /// The server address
        /// </summary>
        private string _serverAddress;

        /// <summary>
        /// The list of server address
        /// </summary>
        private List<string> _serverList;

        /// <summary>
        /// Event Handler, invoke when the user is trying to connect to a server
        /// </summary>
        public EventHandler ConnectToBlockchain;

        /// <summary>
        /// Create the ViewModel with the server list and subscribe to the event handler ServerListUpdated
        /// </summary>
        /// <param name="serverList"></param>
        /// <see cref="MainViewModel.ServerListUpdated"/>
        public ConnectBlockchainViewModel(IEnumerable<string> serverList)
        {
            ConnectToBlockchainCommand = new DelegateCommand(ConnectToBlockchainMethod);

            MainViewModel.ServerListUpdated += ServerListUpdate;
            ServerList = serverList.ToList();
        }

        /// <summary>
        /// Method use when the EventHandler ServerListUpdated is invoke
        /// It update the server list.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <see cref="MainViewModel.ServerListUpdated"/>
        /// <seealso cref="ServerList"/>
        private void ServerListUpdate(object sender, EventArgs e)
        {
            ServerList = MainViewModel.ServerList.Keys.ToList();
        }

        /// <summary>
        /// The ICommand for the button
        /// </summary>
        public ICommand ConnectToBlockchainCommand { get; }

        /// <summary>
        /// Method to invoke ConnectToBlockchain when the user click the button
        /// </summary>
        public void ConnectToBlockchainMethod()
        {
            ConnectToBlockchain?.BeginInvoke(this, new EventArgsMessage(ServerAddress), null, null);
        }

        #region Input

        /// <summary>
        /// The input string ServerAddress
        /// </summary>
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

        /// <summary>
        /// The list of server
        /// </summary>
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