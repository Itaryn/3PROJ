using KittyCoins.Models;
using KittyCoins.ViewModels;
using System;
using System.Windows.Controls;
using System.Windows.Media;

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

            ViewModel.UserChanged += ChangeConnectMessage;
        }

        private void ChangeConnectMessage(object sender, EventArgs e)
        {
            if (ConnectMessage.Text == Constants.WALLET_CONNECTED)
            {
                ConnectMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 91, 193, 70));
            }
            else
            {
                ConnectMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 254, 77, 98));
            }
        }
    }
}
