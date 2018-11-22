using System.ComponentModel;
using System.Windows.Input;
using KittyCoins.Models;
using Prism.Commands;

namespace KittyCoins.ViewModels
{
    public class MainViewModel
    {
        private bool _checkBoxMine;
        private int _port;
        private string _serverUrl;
        public MainViewModel()
        {
            LaunchServerCommand = new DelegateCommand(Mining);

            Port = 0;
            ServerUrl = "127.0.0.1:6002";
        }

        public ICommand LaunchServerCommand { get; }

        #region Mining

        public void Mining()
        {
            var name = "Unknown";
            Client miner;

            if (Port > 0)
            {
                miner = new Client(Port);
            }
            
            while (true)
            {

            }

            //Client.Close();
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
        public string ServerUrl
        {
            get => _serverUrl;
            set
            {
                if (_serverUrl == value) return;
                _serverUrl = value;
                RaisePropertyChanged("ServerUrl");
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