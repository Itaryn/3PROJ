using KittyCoins.ViewModels;
using System.Windows.Controls;

namespace KittyCoins.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ConnectWalletView : UserControl
    {
        public ConnectWalletViewModel ViewModel { get; }

        public ConnectWalletView(string publicAddress)
        {
            InitializeComponent();
            ViewModel = new ConnectWalletViewModel(publicAddress);
            DataContext = ViewModel;
        }
    }
}
