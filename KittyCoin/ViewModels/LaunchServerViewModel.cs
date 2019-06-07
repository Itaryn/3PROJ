using System;
using System.ComponentModel;
using System.Windows.Input;
using KittyCoin.Models;
using Prism.Commands;

namespace KittyCoin.ViewModels
{
    public class LaunchServerViewModel : INotifyPropertyChanged
    {
        private string _port;

        public EventHandler LaunchServerWithPort;

        public LaunchServerViewModel()
        {
            LaunchServerCommand = new DelegateCommand(LaunchServerMethod);
        }

        public ICommand LaunchServerCommand { get; }

        public void LaunchServerMethod()
        {
            LaunchServerWithPort.BeginInvoke(this, new EventArgsMessage(Port), null, null);
        }

        #region Input

        public string Port
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