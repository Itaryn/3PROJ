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

        public BlockChainView()
        {
            InitializeComponent();
            _viewModel = new BlockChainViewModel();
            DataContext = _viewModel;
        }
    }
}
