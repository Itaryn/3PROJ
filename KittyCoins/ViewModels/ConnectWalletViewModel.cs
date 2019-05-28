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

        public EventHandler ConnectWithWords;

        public ConnectWalletViewModel()
        {
            ConnectWithWordsCommand = new DelegateCommand(ConnectWithWordsMethod);
            ConnectWithFileCommand = new DelegateCommand(ConnectWithFileMethod);
        }

        public ICommand ConnectWithWordsCommand { get; }
        public ICommand ConnectWithFileCommand { get; }

        public void ConnectWithWordsMethod()
        {
            ConnectWithWords.Invoke(this, new EventArgsMessage(PrivateWords));
        }

        public void ConnectWithFileMethod()
        {
            var openFileDialog = new OpenFileDialog();

            if (openFileDialog.ShowDialog() == true)
            {
                ConnectWithWords.Invoke(this, new EventArgsMessage(File.ReadAllText(openFileDialog.FileName)));
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