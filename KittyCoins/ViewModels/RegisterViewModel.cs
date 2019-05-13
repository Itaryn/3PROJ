using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Prism.Commands;

namespace KittyCoins.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string _privateKey;

        public RegisterViewModel()
        {
            RefreshWordsCommand = new DelegateCommand(RefreshWordsMethod);
            RefreshWordsMethod();
        }

        public ICommand RefreshWordsCommand { get; }

        public void RefreshWordsMethod()
        {
            var wordDictionnary = File.ReadAllLines(@".\Resources\File\wordDictionnary.txt");
            var rand = new Random();
            var wordList = new List<string>();
            for (var i = 0; i < 10; i++)
                wordList.Add(wordDictionnary[rand.Next(wordDictionnary.Length)]);
            PrivateKey = string.Join(" ", wordList);
        }

        #region Input

        public string PrivateKey
        {
            get => _privateKey;
            set
            {
                if (_privateKey == value) return;
                _privateKey = value;
                RaisePropertyChanged("PrivateKey");
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