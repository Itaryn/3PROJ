using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
        private bool _checkBoxMine;
        private int _port;
        private string _peerUrl;
        private string _consoleOutput = "";
        public static List<string> MessageFromClientOrServer = new List<string>();
        public static KittyChain BlockChain = new KittyChain();
        private Client Client;
        private Server Server;
        public MainViewModel()
        {
            LaunchServerCommand = new DelegateCommand(LaunchServerMethod);
            ShowBlockChainCommand = new DelegateCommand(ShowBlockChainMethod);
            NewTransactionCommand = new DelegateCommand(NewTransactionMethod);
            Port = 0;
            PeerUrl = "127.0.0.1:6002";
        }

        public ICommand LaunchServerCommand { get; }
        public ICommand ShowBlockChainCommand { get; }
        public ICommand NewTransactionCommand { get; }

        #region Mining

        public void LaunchServerMethod()
        {
            var thread = new Thread(Mining);
            thread.Start();
        }
        public void Mining()
        {
            var name = "Unknown";
            Client = new Client();
            Console = "Create Client";
            Server = new Server();
            Console = "Create Server";
            Server.Start(Port);
            Console = $"Start server with port n°{Port}";


            while (CheckBoxMine)
            {
                if (MessageFromClientOrServer != null && MessageFromClientOrServer.Count != 0)
                {
                    Console = MessageFromClientOrServer.First();
                    MessageFromClientOrServer.RemoveAt(0);
                }
                Thread.Sleep(10);
            }

            Client.Close();
            Console = "Client close";
            Server.wss.Stop();
            Console = "Server close";
        }
        #endregion

        public void ShowBlockChainMethod()
        {
            Console = BlockChain.ToString();
        }

        public void NewTransactionMethod()
        {
            Client.Connect("ws://127.0.0.1:6002/Blockchain");
            var transfer = new Transfer("Guilhem", "Loic", 15, 1);
            BlockChain.CreateTransfer(transfer);
            Client.Send("ws://127.0.0.1:6002/Blockchain", "Transfer" + JsonConvert.SerializeObject(transfer));
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