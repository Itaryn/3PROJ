using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using KittyCoins.Models;
using Newtonsoft.Json;
using Prism.Commands;
using WebSocketSharp;

namespace KittyCoins.ViewModels
{
    public class MainViewModel
    {
        private bool _checkBoxMine;
        private int _port;
        private string _peerUrl;
        private string _consoleOutput = "";
        public static KittyChain BlockChain = new KittyChain();
        private Client Client;
        public MainViewModel()
        {
            LaunchServerCommand = new DelegateCommand(LaunchServerMethod);
            NewTransactionCommand = new DelegateCommand(NewTransactionMethod);

            Port = 0;
            PeerUrl = "127.0.0.1:6002";
        }

        public ICommand LaunchServerCommand { get; }
        public ICommand NewTransactionCommand { get; }

        #region Mining

        public void LaunchServerMethod()
        {
            var test = new Thread(Mining);
            test.Start();
        }
        public void Mining()
        {
            var name = "Unknown";
            Client = new Client();
            var Server = new Server();
            Server.Start(Port);


            while (CheckBoxMine)
            {
                Thread.Sleep(1000);
            }

            Client.Close();
            Server.wss.Stop();
        }
        #endregion

        public void NewTransactionMethod()
        {
            Client.Connect("ws://127.0.0.1:6002/Blockchain");
            Client.Send("ws://127.0.0.1:6002/Blockchain", JsonConvert.SerializeObject(BlockChain));
        }

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