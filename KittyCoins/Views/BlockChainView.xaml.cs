namespace KittyCoins.Views
{
    using KittyCoins.Models;
    using System.Windows;
    using ViewModels;

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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.UpdateThread?.Abort();
            base.OnClosing(e);
        }
    }
}
