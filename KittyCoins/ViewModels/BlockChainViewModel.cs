namespace KittyCoins.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using Models;

    public class BlockChainViewModel : INotifyPropertyChanged
    {
        #region Private Attributes

        private Block _selectedBlock;

        private KittyChain blockChain;

        #endregion

        #region Public Attributes

        public Thread UpdateThread;

        #endregion

        #region Constructors

        public BlockChainViewModel(KittyChain blockchain)
        {
            blockChain = blockchain;
        }

        #endregion

        #region Public Methods

        public void UpdateBlockChain()
        {
            UpdateThread = new Thread(UpdateBlockChainThread) { IsBackground = true };
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

        #endregion
        
        #region Input
        public KittyChain BlockChain => blockChain;
        public List<Transfer> PendingTransfers => blockChain.PendingTransfers.ToList();
        public List<Block> Chain => blockChain.Chain.ToList();

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