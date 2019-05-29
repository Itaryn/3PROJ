using KittyCoins.ViewModels;
using System.Collections.Generic;
using System.Windows.Controls;

namespace KittyCoins.Views
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ConnectBlockchainView : UserControl
    {
        public ConnectBlockchainViewModel ViewModel;

        public ConnectBlockchainView(IEnumerable<string> serverList)
        {
            InitializeComponent();
            ViewModel = new ConnectBlockchainViewModel(serverList);
            DataContext = ViewModel;
        }
    }
}
