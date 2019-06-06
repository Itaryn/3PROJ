using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using KittyCoin.Models;
using KittyCoin.ViewModels;

namespace KittyCoin.Views
{
    /// <summary>
    /// Interaction logic for ConnectBlockchainView.xaml
    /// </summary>
    public partial class ConnectBlockchainView : UserControl
    {
        public ConnectBlockchainViewModel ViewModel;

        public ConnectBlockchainView(IEnumerable<string> serverList, Server server)
        {
            InitializeComponent();
            ViewModel = new ConnectBlockchainViewModel(serverList);
            DataContext = ViewModel;

            if (server == null)
            {
                ConnectButton.IsEnabled = false;
                ServerStatusMessage.Text = "Launch your server with a port before connecting to the KittyCoin Network";
                ServerStatusMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 254, 77, 98));
            }
            else
            {
                ConnectButton.IsEnabled = true;
                ServerStatusMessage.Text = "Your server is launched, enter an ip to connect to the KittyCoin Network";
                ServerStatusMessage.Foreground = new SolidColorBrush(Color.FromArgb(255, 91, 193, 70));
            }
        }
    }
}
