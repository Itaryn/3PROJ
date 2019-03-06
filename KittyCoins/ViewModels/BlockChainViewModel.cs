namespace KittyCoins.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using Models;

    public class BlockChainViewModel : INotifyPropertyChanged
    {
        private Block _selectedBlock;

        public Thread UpdateThread;

        public BlockChainViewModel()
        {
            UpdateBlockChain();
        }

        public void UpdateBlockChain()
        {
            UpdateThread = new Thread(UpdateBlockChainThread) {IsBackground = true};
            UpdateThread.Start();
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