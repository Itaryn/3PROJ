using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using KittyCoin.Models;

namespace KittyCoin.ViewModels
{
    public class ShowBlockchainViewModel : INotifyPropertyChanged
    {
        #region Private Attributes

        private Block _selectedBlock;

        #endregion

        #region Public Attributes

        public Thread UpdateThread;

        #endregion

        #region Constructors

        public ShowBlockchainViewModel()
        {
            MainViewModel.BlockChainUpdated += UpdateBlockChain;
        }

        #endregion

        #region Public Methods

        public void UpdateBlockChain(object sender, EventArgs e)
        {
            RaisePropertyChanged("BlockChain");
            RaisePropertyChanged("PendingTransfers");
            RaisePropertyChanged("Chain");
        }

        #endregion
        
        #region Input
        public KittyChain BlockChain => MainViewModel.BlockChain;
        public List<Transfer> PendingTransfers => MainViewModel.BlockChain.PendingTransfers.ToList();
        public List<Block> Chain => MainViewModel.BlockChain.Chain.ToList();

        public Block SelectedBlock
        {
            get => _selectedBlock;
            set
            {
                if (value != null &&
                    value.Equals(_selectedBlock)) return;
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