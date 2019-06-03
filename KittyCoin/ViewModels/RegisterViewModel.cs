using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Prism.Commands;

namespace KittyCoin.ViewModels
{
    public class RegisterViewModel : INotifyPropertyChanged
    {
        private string _privateKey;

        public RegisterViewModel()
        {
            RefreshWordsCommand = new DelegateCommand(RefreshWordsMethod);
            SaveItInFileCommand = new DelegateCommand(SaveItInFileMethod);
            RefreshWordsMethod();
        }

        public ICommand RefreshWordsCommand { get; }
        public ICommand SaveItInFileCommand { get; }

        public void RefreshWordsMethod()
        {
            var wordDictionnary = File.ReadAllLines(@".\Resources\File\wordDictionnary.txt");
            var rand = new Random();
            var wordList = new List<string>();
            for (var i = 0; i < 10; i++)
                wordList.Add(wordDictionnary[rand.Next(wordDictionnary.Length)]);
            PrivateKey = string.Join(" ", wordList);
        }

        public void SaveItInFileMethod()
        {
            var saveFileDialog = new SaveFileDialog();

            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, PrivateKey);
            }
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