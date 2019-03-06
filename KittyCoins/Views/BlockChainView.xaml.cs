namespace KittyCoins.Views
{
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _viewModel.UpdateThread?.Abort();
            base.OnClosing(e);
        }
    }
}
