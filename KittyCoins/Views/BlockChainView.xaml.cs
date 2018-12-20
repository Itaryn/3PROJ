using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using KittyCoins.ViewModels;

namespace KittyCoins.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class BlockChainView : Window
    {
        private BlockChainViewModel _viewModel;
        public BlockChainView()
        {
            InitializeComponent();
            _viewModel = new BlockChainViewModel();
            DataContext = _viewModel;
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.updateThread?.Abort();
            base.OnClosing(e);
        }
    }
}
