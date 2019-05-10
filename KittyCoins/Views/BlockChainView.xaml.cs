using System.ComponentModel;
using System.Windows;
using KittyCoins.Models;
using KittyCoins.ViewModels;

namespace KittyCoins.Views
{
    /// <summary>
    /// Interaction logic for BlockChainView.xaml
    /// </summary>
    public partial class BlockChainView : Window
    {
        private readonly BlockChainViewModel _viewModel;

        public BlockChainView(KittyChain blockchain)
        {
            InitializeComponent();
            _viewModel = new BlockChainViewModel(blockchain);
            DataContext = _viewModel;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            _viewModel.UpdateThread?.Abort();
            base.OnClosing(e);
        }
    }
}
