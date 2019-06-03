using System.Windows.Controls;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for ShowBlockchainView.xaml
    /// </summary>
    public partial class ShowBlockchainView : UserControl
    {
        private ShowBlockchainViewModel _viewModel;

        public ShowBlockchainView()
        {
            InitializeComponent();
            _viewModel = new ShowBlockchainViewModel();
            DataContext = _viewModel;
        }
    }
}
