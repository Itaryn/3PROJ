using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using KittyCoins.Models;
using Prism.Commands;

namespace KittyCoins.ViewModels
{
    public class LaunchServerViewModel : INotifyPropertyChanged
    {
        private int _port;

        public EventHandler LaunchServerWithPort;

        public LaunchServerViewModel()
        {
            LaunchServerCommand = new DelegateCommand(LaunchServerMethod);
        }

        public ICommand LaunchServerCommand { get; }

        public void LaunchServerMethod()
        {
            LaunchServerWithPort.Invoke(this, new EventArgsMessage(Port.ToString()));
        }

        #region Input

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