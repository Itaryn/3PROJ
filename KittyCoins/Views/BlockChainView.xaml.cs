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

        public BlockChainView()
        {
            InitializeComponent();
            _viewModel = new BlockChainViewModel();
            DataContext = _viewModel;
        }
    }
}
