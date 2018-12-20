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
    public class BlockChainViewModel : INotifyPropertyChanged
    {
        private Block _selectedBlock;
        public Thread updateThread;
        public BlockChainViewModel()
        {
            UpdateBlockChain();
        }

        public void UpdateBlockChain()
        {
            updateThread = new Thread(UpdateBlockChainThread) {IsBackground = true};
            updateThread.Start();
        }

        public void UpdateBlockChainThread()
        {
            while (true)
            {
                // Check every second
                Thread.Sleep(1000);
                RaisePropertyChanged("BlockChain");
                RaisePropertyChanged("PendingTransfers");
                RaisePropertyChanged("Chain");
            }
        }
        #region Input
        public KittyChain BlockChain => MainViewModel.BlockChain;
        public List<Transfer> PendingTransfers => MainViewModel.BlockChain.PendingTransfers.ToList();
        public List<Block> Chain => MainViewModel.BlockChain.Chain.ToList();

        public Block SelectedBlock
        {
            get => _selectedBlock;
            set
            {
                if (_selectedBlock == value) return;
                _selectedBlock = value;
                RaisePropertyChanged("SelectedBlock");
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