using KittyCoins.ViewModels;
using System.Windows.Controls;

namespace KittyCoins.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ConnectBlockchainView : UserControl
    {
        public ConnectBlockchainViewModel ViewModel;

        public ConnectBlockchainView()
        {
            InitializeComponent();
            ViewModel = new ConnectBlockchainViewModel();
            DataContext = ViewModel;
        }
    }
}
